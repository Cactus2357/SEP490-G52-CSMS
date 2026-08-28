Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Updating CSMS Staging Docker Container (Port 9000)..." -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
docker compose up -d --build
Write-Host "`nContainer updated successfully! Available at: http://localhost:9000" -ForegroundColor Green
