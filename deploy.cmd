@echo off
:: deploy.cmd — Build and publish CRS Reporter for production (Windows)
::
:: Usage:
::   deploy.cmd [output-dir]
::
:: Default output: dist\ (relative to this script)
::
:: Requirements:
::   - Node.js + npm  (for the Vue UI)
::   - .NET 10 SDK    (for the C# server)
::
:: After running this script, start the server with:
::   cd dist
::   set CRS_ENV=production && Server.exe
::
:: Optional env vars at runtime:
::   CRS_PORT=8080    — listen on a different port (default: 9876)
::   CRS_ENV=production — disables developer error pages
::
:: SPA deep-link note:
::   Vue Router uses HTML5 history mode. Direct navigation to /pages/... requires
::   a reverse proxy that rewrites unknown paths to index.html. Consider IIS URL
::   Rewrite or put Caddy in front for automatic HTTPS + SPA routing.
::
:: ─────────────────────────────────────────────────────────────────────────────

setlocal EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
:: Remove trailing backslash
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

set "OUTPUT_DIR=%~1"
if "%OUTPUT_DIR%"=="" set "OUTPUT_DIR=%SCRIPT_DIR%\dist"

echo.
echo  CRS Reporter - Production Build
echo  ================================
echo.
echo   Output: %OUTPUT_DIR%
echo.

:: ── 1. Vue UI ─────────────────────────────────────────────────────────────────
echo [1/2] Building Vue UI for production...
cd /d "%SCRIPT_DIR%\ui"
call npm ci --prefer-offline
if errorlevel 1 (
    echo ERROR: npm ci failed
    exit /b 1
)
call npm run build
if errorlevel 1 (
    echo ERROR: npm run build failed
    exit /b 1
)
echo   OK  Vue build complete ^> %SCRIPT_DIR%\Resources
echo.

:: ── 2. .NET server ────────────────────────────────────────────────────────────
echo [2/2] Publishing .NET server (Release)...
cd /d "%SCRIPT_DIR%"
dotnet publish Server\Server.csproj ^
    --configuration Release ^
    --output "%OUTPUT_DIR%" ^
    --no-self-contained
if errorlevel 1 (
    echo ERROR: dotnet publish failed
    exit /b 1
)
echo   OK  Server published ^> %OUTPUT_DIR%
echo.

:: ── Done ──────────────────────────────────────────────────────────────────────
echo  Build complete!
echo  ===============
echo.
echo   Start the server:
echo     cd "%OUTPUT_DIR%"
echo     set CRS_ENV=production ^&^& Server.exe
echo.
echo   Or with a custom port:
echo     set CRS_PORT=8080 ^&^& set CRS_ENV=production ^&^& Server.exe
echo.

endlocal
