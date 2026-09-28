@echo off
echo ========================================================
echo Updating CSMS Staging Docker Container (Port 9000)...
echo ========================================================
echo.
docker compose up -d --build
echo.
echo ========================================================
echo Container services started!
echo Local Web:     https://localhost:9000
echo Minio Console: http://localhost:9003
echo Ngrok Web UI:  http://localhost:4040
echo ========================================================
echo.
powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Sleep -Seconds 3; try { $res = Invoke-RestMethod -Uri 'http://localhost:4040/api/tunnels' -TimeoutSec 3; if ($res.tunnels.Count -gt 0) { Write-Host '>>> Public Ngrok URL: ' -NoNewline -ForegroundColor Green; Write-Host $res.tunnels[0].public_url -ForegroundColor Yellow } else { Write-Host '>>> Ngrok is running, check http://localhost:4040' -ForegroundColor Cyan } } catch { Write-Host '>>> Ngrok starting... (Wait a few seconds or check http://localhost:4040)' -ForegroundColor Cyan }"
echo.
pause
