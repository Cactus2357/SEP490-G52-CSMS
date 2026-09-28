<#
.SYNOPSIS
    Migrates files from wwwroot/uploads/ to MinIO S3 and updates references in the database.
#>
param(
    [string]$AppSettingsPath = "appsettings.json"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Starting Uploads Migration to Object Storage ===" -ForegroundColor Cyan

# 1. Read appsettings.json
if (-not (Test-Path $AppSettingsPath)) {
    throw "Configuration file not found: $AppSettingsPath"
}

$config = Get-Content $AppSettingsPath -Raw | ConvertFrom-Json
$connStr = $config.ConnectionStrings.DefaultConnection
$s3ConfigSection = $config.S3Storage

$serviceUrl = if ($s3ConfigSection.ServiceUrl) { $s3ConfigSection.ServiceUrl } else { "http://localhost:9002" }
$bucketName = if ($s3ConfigSection.BucketName) { $s3ConfigSection.BucketName } else { "csms-storage" }
$accessKey  = if ($s3ConfigSection.AccessKey) { $s3ConfigSection.AccessKey } else { "minioadmin" }
$secretKey  = if ($s3ConfigSection.SecretKey) { $s3ConfigSection.SecretKey } else { "minioadmin123" }

Write-Host "Target S3 ServiceUrl: $serviceUrl"
Write-Host "Target Bucket:        $bucketName"
Write-Host "Connection String:    $connStr"

# 2. Load AWS SDK
$binDebugPath = "bin\Debug\net8.0"
$awsCoreDll = Join-Path $binDebugPath "AWSSDK.Core.dll"
$awsS3Dll   = Join-Path $binDebugPath "AWSSDK.S3.dll"

if (-not (Test-Path $awsCoreDll) -or -not (Test-Path $awsS3Dll)) {
    throw "AWS SDK assemblies not found in $binDebugPath. Please build project first."
}

Add-Type -Path $awsCoreDll
Add-Type -Path $awsS3Dll

$s3Config = [Amazon.S3.AmazonS3Config]::new()
$s3Config.ServiceURL = $serviceUrl
$s3Config.ForcePathStyle = $true
$s3Config.UseHttp = $serviceUrl.StartsWith("http://", [System.StringComparison]::OrdinalIgnoreCase)

$s3Client = [Amazon.S3.AmazonS3Client]::new($accessKey, $secretKey, $s3Config)

# 3. Ensure Bucket & Policy
Write-Host "`nEnsuring bucket '$bucketName' exists..." -ForegroundColor Yellow
$bucketExists = [Amazon.S3.Util.AmazonS3Util]::DoesS3BucketExistV2Async($s3Client, $bucketName).GetAwaiter().GetResult()
if (-not $bucketExists) {
    $putBucket = [Amazon.S3.Model.PutBucketRequest]::new()
    $putBucket.BucketName = $bucketName
    $s3Client.PutBucketAsync($putBucket).GetAwaiter().GetResult() | Out-Null
    Write-Host "Created bucket: $bucketName" -ForegroundColor Green
} else {
    Write-Host "Bucket already exists." -ForegroundColor Green
}

# Set public read policy
try {
    $policyJson = "{`"Version`":`"2012-10-17`",`"Statement`":[{`"Sid`":`"PublicReadGetObject`",`"Effect`":`"Allow`",`"Principal`":`"*`",`"Action`":[`"s3:GetObject`"],`"Resource`":[`"arn:aws:s3:::$bucketName/*`"]}]}"
    $policyReq = [Amazon.S3.Model.PutBucketPolicyRequest]::new()
    $policyReq.BucketName = $bucketName
    $policyReq.Policy = $policyJson
    $s3Client.PutBucketPolicyAsync($policyReq).GetAwaiter().GetResult() | Out-Null
    Write-Host "Bucket public read policy verified." -ForegroundColor Green
} catch {
    Write-Warning "Could not set bucket policy: $_"
}

# 4. Upload Files from wwwroot/uploads
$uploadsDir = "wwwroot\uploads"
if (-not (Test-Path $uploadsDir)) {
    Write-Warning "Directory $uploadsDir does not exist."
    return
}

$files = Get-ChildItem -Path $uploadsDir -Recurse -File
Write-Host "`nFound $($files.Count) files in $uploadsDir to migrate..." -ForegroundColor Yellow

$uploadedCount = 0
$skippedCount = 0

function Get-ContentType([string]$extension) {
    switch ($extension.ToLowerInvariant()) {
        ".jpg"  { return "image/jpeg" }
        ".jpeg" { return "image/jpeg" }
        ".png"  { return "image/png" }
        ".webp" { return "image/webp" }
        ".gif"  { return "image/gif" }
        ".pdf"  { return "application/pdf" }
        default { return "application/octet-stream" }
    }
}

$baseUploadsResolved = (Resolve-Path $uploadsDir).Path

foreach ($file in $files) {
    # Compute relative key, e.g. products/filename.jpg
    $fullPath = $file.FullName
    $relPath = $fullPath.Substring($baseUploadsResolved.Length).TrimStart('\', '/')
    $s3Key = $relPath.Replace('\', '/')

    $contentType = Get-ContentType $file.Extension

    try {
        $putReq = [Amazon.S3.Model.PutObjectRequest]::new()
        $putReq.BucketName = $bucketName
        $putReq.Key = $s3Key
        $putReq.FilePath = $fullPath
        $putReq.ContentType = $contentType

        $s3Client.PutObjectAsync($putReq).GetAwaiter().GetResult() | Out-Null
        Write-Host " [UPLOADED] $s3Key ($contentType, $($file.Length) bytes)" -ForegroundColor Green
        $uploadedCount++
    } catch {
        Write-Error "Failed to upload $s3Key : $_"
    }
}

Write-Host "`nUploaded $uploadedCount files to MinIO S3." -ForegroundColor Cyan

# 5. Update Database Records
Write-Host "`n=== Updating Database Records ===" -ForegroundColor Yellow
$sqlConn = [System.Data.SqlClient.SqlConnection]::new($connStr)
$sqlConn.Open()

$useRelative = if ($s3ConfigSection.UseRelativeUrl -ne $null) { [bool]$s3ConfigSection.UseRelativeUrl } else { $true }
$targetPrefix = if ($useRelative) { "/$bucketName/" } else { "$($serviceUrl.TrimEnd('/'))/$bucketName/" }
Write-Host "URL Prefix for Database: $targetPrefix"

# Update master_products
$cmd = $sqlConn.CreateCommand()
$cmd.CommandText = @"
UPDATE master_products 
SET image_url = REPLACE(REPLACE(image_url, '/uploads/', @prefix), 'http://localhost:9002/csms-storage/', @prefix) 
WHERE image_url LIKE '%/uploads/%' OR image_url LIKE 'http://localhost:9002/csms-storage/%'
"@
$cmd.Parameters.AddWithValue("@prefix", $targetPrefix) | Out-Null
$prodUpdated = $cmd.ExecuteNonQuery()
Write-Host "Updated master_products (image_url): $prodUpdated rows" -ForegroundColor Green

# Update employees cccd_file_path
$cmd = $sqlConn.CreateCommand()
$cmd.CommandText = @"
UPDATE employees 
SET cccd_file_path = REPLACE(REPLACE(cccd_file_path, '/uploads/', @prefix), 'http://localhost:9002/csms-storage/', @prefix) 
WHERE cccd_file_path LIKE '%/uploads/%' OR cccd_file_path LIKE 'http://localhost:9002/csms-storage/%'
"@
$cmd.Parameters.AddWithValue("@prefix", $targetPrefix) | Out-Null
$cccdUpdated = $cmd.ExecuteNonQuery()
Write-Host "Updated employees (cccd_file_path): $cccdUpdated rows" -ForegroundColor Green

# Update employees contract_file_path
$cmd = $sqlConn.CreateCommand()
$cmd.CommandText = @"
UPDATE employees 
SET contract_file_path = REPLACE(REPLACE(contract_file_path, '/uploads/', @prefix), 'http://localhost:9002/csms-storage/', @prefix) 
WHERE contract_file_path LIKE '%/uploads/%' OR contract_file_path LIKE 'http://localhost:9002/csms-storage/%'
"@
$cmd.Parameters.AddWithValue("@prefix", $targetPrefix) | Out-Null
$contractUpdated = $cmd.ExecuteNonQuery()
Write-Host "Updated employees (contract_file_path): $contractUpdated rows" -ForegroundColor Green

# Update branch_supply_request_items defect_image_url
$cmd = $sqlConn.CreateCommand()
$cmd.CommandText = @"
UPDATE branch_supply_request_items 
SET defect_image_url = REPLACE(REPLACE(defect_image_url, '/uploads/', @prefix), 'http://localhost:9002/csms-storage/', @prefix) 
WHERE defect_image_url LIKE '%/uploads/%' OR defect_image_url LIKE '%http://localhost:9002/csms-storage/%'
"@
$cmd.Parameters.AddWithValue("@prefix", $targetPrefix) | Out-Null
$defectUpdated = $cmd.ExecuteNonQuery()
Write-Host "Updated branch_supply_request_items (defect_image_url): $defectUpdated rows" -ForegroundColor Green

$sqlConn.Close()

# 6. Verification
Write-Host "`n=== Verifying Sample HTTP Access ===" -ForegroundColor Cyan
$sampleUrls = @(
    "$serviceUrl/$bucketName/employees/cccd_6_112de7ef-7fab-43a1-a95f-93f3dfd69a93.png",
    "$serviceUrl/$bucketName/products/f7194337600a4ae28eba3617e208c9bf.webp",
    "$serviceUrl/$bucketName/supply_defects/2652b47f0ff341838c2c92335ffb082c.jpg"
)

foreach ($testUrl in $sampleUrls) {
    try {
        $resp = Invoke-WebRequest -Uri $testUrl -Method Head -UseBasicParsing -TimeoutSec 5
        Write-Host " [HTTP OK $($resp.StatusCode)] $testUrl (Content-Type: $($resp.Headers['Content-Type']))" -ForegroundColor Green
    } catch {
        Write-Warning " [HTTP FAIL] $testUrl : $_"
    }
}

Write-Host "`n=== Migration Completed Successfully! ===" -ForegroundColor Cyan
