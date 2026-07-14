@echo off
REM EventMarketplace Application Launcher
REM Runs both API and Web (Blazor) projects in separate terminals

set "API_HTTP=http://localhost:5105"
set "API_HTTPS=https://localhost:7170"
set "WEB_HTTP=http://localhost:5002"

echo.
echo ============================================
echo   EventMarketplace Application Launcher
echo ============================================
echo.
echo  API  : %API_HTTPS% (and %API_HTTP%)
echo  Web  : %WEB_HTTP%
echo.
echo  Press CTRL+C in each terminal to stop.
echo.

REM Start API in a new terminal (https profile => ports 7170 / 5105)
start "EventMarketplace.API" cmd /k "cd /d %~dp0EventMarketplace.API && dotnet run --launch-profile https"

REM Wait for API to start (ping-based delay works in non-interactive shells)
echo Waiting for API to start...
ping -n 10 127.0.0.1 >nul 2>&1

REM Start Web in a new terminal (default port 5002)
start "EventMarketplace.Web" cmd /k "cd /d %~dp0EventMarketplace.Web && dotnet run --urls %WEB_HTTP%"

echo.
echo Both projects are starting...
echo.

REM Give the Web project a moment to boot
ping -n 12 127.0.0.1 >nul 2>&1

echo Checking endpoints...
echo.

for /f %%I in ('powershell -NoProfile -ExecutionPolicy Bypass -Command "try { (Invoke-WebRequest -Uri '%API_HTTP%/swagger/index.html' -UseBasicParsing -TimeoutSec 15).StatusCode } catch { 0 }"') do set API_STATUS=%%I
for /f %%I in ('powershell -NoProfile -ExecutionPolicy Bypass -Command "try { (Invoke-WebRequest -Uri '%WEB_HTTP%' -UseBasicParsing -TimeoutSec 15).StatusCode } catch { 0 }"') do set WEB_STATUS=%%I

if "%API_STATUS%"=="200" (
	echo  [OK]   API    : %API_HTTP%
) else (
	echo  [WAIT] API    : %API_HTTP%  ^(status: %API_STATUS% - henuz hazirlaniyor^)
)

if "%WEB_STATUS%"=="200" (
	echo  [OK]   Web    : %WEB_HTTP%
) else (
	echo  [WAIT] Web    : %WEB_HTTP%  ^(status: %WEB_STATUS% - henuz hazirlaniyor^)
)

echo.
echo ============================================
echo   Erisim Adresleri
echo ============================================
echo.
echo   Swagger : %API_HTTP%/swagger/index.html
echo   Web     : %WEB_HTTP%
echo   API     : %API_HTTP%
echo.
echo   MariaDB : 192.168.0.18:3306
echo   DB User : root
echo   DB Name : EventMarketplaceDb
echo.
echo ============================================
echo   Projeler baslatildi. Iyi calismalar!
echo ============================================
echo.
