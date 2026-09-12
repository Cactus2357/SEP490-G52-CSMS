using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SEP490_G52_CSMS.Controllers
{
    [AllowAnonymous]
    [Route("csms-storage")]
    public class StorageProxyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<StorageProxyController> _logger;

        public StorageProxyController(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            IWebHostEnvironment env,
            ILogger<StorageProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _env = env;
            _logger = logger;
        }

        [HttpGet("{*path}")]
        public async Task<IActionResult> GetFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Contains(".."))
            {
                return BadRequest("Đường dẫn không hợp lệ.");
            }

            var cleanPath = path.TrimStart('/');
            var s3Section = _config.GetSection("S3Storage");
            var serviceUrl = s3Section["ServiceUrl"] ?? "http://localhost:9002";
            var bucketName = s3Section["BucketName"] ?? "csms-storage";

            var targetMinioUrl = $"{serviceUrl.TrimEnd('/')}/{bucketName}/{cleanPath}";

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, targetMinioUrl);

                if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch))
                {
                    request.Headers.TryAddWithoutValidation("If-None-Match", (string?)ifNoneMatch);
                }
                if (Request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSince))
                {
                    request.Headers.TryAddWithoutValidation("If-Modified-Since", (string?)ifModifiedSince);
                }

                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    return StatusCode((int)HttpStatusCode.NotModified);
                }

                if (response.IsSuccessStatusCode)
                {
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? GetFallbackContentType(cleanPath);

                    if (response.Headers.ETag != null)
                    {
                        Response.Headers["ETag"] = response.Headers.ETag.ToString();
                    }
                    if (response.Content.Headers.LastModified.HasValue)
                    {
                        Response.Headers["Last-Modified"] = response.Content.Headers.LastModified.Value.ToString("R");
                    }

                    // Cache in browser for 24 hours
                    Response.Headers["Cache-Control"] = "public, max-age=86400";

                    var stream = await response.Content.ReadAsStreamAsync();
                    return File(stream, contentType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not proxy file request from MinIO for path {Path}", cleanPath);
            }

            // Fallback to local file in wwwroot/uploads/{cleanPath} if exists
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var localPath = Path.Combine(webRoot, "uploads", cleanPath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(localPath))
            {
                return PhysicalFile(localPath, GetFallbackContentType(cleanPath));
            }

            return NotFound();
        }

        private static string GetFallbackContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
