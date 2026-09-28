Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Updating CSMS Staging Docker Container (Port 9000)..." -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
docker compose up -d --build

Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "Container services started!" -ForegroundColor Green
Write-Host "Local Web:     https://localhost:9000"
Write-Host "Minio Console: http://localhost:9003"
Write-Host "Ngrok Web UI:  http://localhost:4040"
Write-Host "========================================================`n" -ForegroundColor Cyan

Start-Sleep -Seconds 3
try {
    $res = Invoke-RestMethod -Uri 'http://localhost:4040/api/tunnels' -TimeoutSec 3
    if ($res.tunnels.Count -gt 0) {
        Write-Host ">>> Public Ngrok URL: " -NoNewline -ForegroundColor Green
        Write-Host $res.tunnels[0].public_url -ForegroundColor Yellow
    } else {
        Write-Host ">>> Ngrok is running, check http://localhost:4040" -ForegroundColor Cyan
    }
} catch {
    Write-Host ">>> Ngrok starting... (Wait a few seconds or check http://localhost:4040)" -ForegroundColor Cyan
}
