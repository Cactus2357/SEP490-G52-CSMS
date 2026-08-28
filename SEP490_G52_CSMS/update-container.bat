@echo off
echo ========================================================
echo Updating CSMS Staging Docker Container (Port 9000)...
echo ========================================================
echo.
docker compose up -d --build
echo.
echo ========================================================
echo Container updated successfully! Available at: http://localhost:9000
echo ========================================================
pause
