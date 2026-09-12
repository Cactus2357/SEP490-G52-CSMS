using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SEP490_G52_CSMS.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Services
{
    public class FileStorageService : IFileStorageService
    {
        private static bool _policyEnsured = false;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IConfiguration config, IWebHostEnvironment env, ILogger<FileStorageService> logger)
        {
            _config = config;
            _env = env;
            _logger = logger;
        }

        private AmazonS3Client? CreateS3Client()
        {
            var s3Section = _config.GetSection("S3Storage");
            if (!s3Section.GetValue<bool>("Enabled", false)) return null;

            var serviceUrl = s3Section["ServiceUrl"] ?? "http://localhost:9002";
            var accessKey = s3Section["AccessKey"] ?? "minioadmin";
            var secretKey = s3Section["SecretKey"] ?? "minioadmin123";
            var forcePathStyle = s3Section.GetValue<bool>("ForcePathStyle", true);

            var s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = forcePathStyle,
                UseHttp = serviceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            };

            return new AmazonS3Client(accessKey, secretKey, s3Config);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var relativeKey = $"{folder}/{fileName}";

            var s3Section = _config.GetSection("S3Storage");
            bool s3Enabled = s3Section.GetValue<bool>("Enabled", false);
            bool fallbackToLocal = s3Section.GetValue<bool>("FallbackToLocalStorage", true);

            if (s3Enabled)
            {
                try
                {
                    using var s3Client = CreateS3Client();
                    if (s3Client != null)
                    {
                        var bucketName = s3Section["BucketName"] ?? "csms-storage";

                        // Ensure bucket exists
                        bool bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
                        if (!bucketExists)
                        {
                            await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });
                        }

                        if (!_policyEnsured)
                        {
                            bool autoApplyPolicy = s3Section.GetValue<bool>("AutoApplyPolicy", true);
                            if (autoApplyPolicy)
                            {
                                try
                                {
                                    string? configuredPolicy = s3Section["Policy"];
                                    string policyJson = !string.IsNullOrWhiteSpace(configuredPolicy)
                                        ? configuredPolicy.Replace("{BucketName}", bucketName)
                                        : $"{{\"Version\":\"2012-10-17\",\"Statement\":[{{\"Sid\":\"PublicReadGetObject\",\"Effect\":\"Allow\",\"Principal\":\"*\",\"Action\":[\"s3:GetObject\"],\"Resource\":[\"arn:aws:s3:::{bucketName}/*\"]}}]}}";

                                    await s3Client.PutBucketPolicyAsync(new PutBucketPolicyRequest
                                    {
                                        BucketName = bucketName,
                                        Policy = policyJson
                                    });
                                    _logger.LogInformation("Applied S3 bucket policy from configuration for bucket {Bucket}", bucketName);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Could not set bucket policy on bucket {Bucket}", bucketName);
                                }
                            }
                            _policyEnsured = true;
                        }

                        using var stream = file.OpenReadStream();
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            Key = relativeKey,
                            InputStream = stream,
                            ContentType = file.ContentType
                        };

                        await s3Client.PutObjectAsync(putRequest);

                        var serviceUrl = s3Section["ServiceUrl"] ?? "http://localhost:9002";
                        return $"{serviceUrl.TrimEnd('/')}/{bucketName}/{relativeKey}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "S3 upload failed for {Key}. FallbackToLocal: {Fallback}", relativeKey, fallbackToLocal);
                    if (!fallbackToLocal) throw;
                }
            }

            // Fallback to local storage in wwwroot/uploads/{folder}
            return await SaveLocallyAsync(file, folder, fileName);
        }

        private async Task<string> SaveLocallyAsync(IFormFile file, string folder, string fileName)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var targetDir = Path.Combine(webRoot, "uploads", folder);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var filePath = Path.Combine(targetDir, fileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return $"/uploads/{folder}/{fileName}";
        }

        public async Task<bool> DeleteFileAsync(string fileUrlOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrPath)) return false;

            try
            {
                if (fileUrlOrPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                    var localPath = Path.Combine(webRoot, fileUrlOrPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(localPath))
                    {
                        File.Delete(localPath);
                        return true;
                    }
                    return false;
                }

                using var s3Client = CreateS3Client();
                if (s3Client != null)
                {
                    var s3Section = _config.GetSection("S3Storage");
                    var bucketName = s3Section["BucketName"] ?? "csms-storage";

                    var uri = new Uri(fileUrlOrPath);
                    var key = uri.AbsolutePath.TrimStart('/');
                    if (key.StartsWith(bucketName + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        key = key.Substring(bucketName.Length + 1);
                    }

                    await s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting file {Path}", fileUrlOrPath);
            }

            return false;
        }
    }
}
