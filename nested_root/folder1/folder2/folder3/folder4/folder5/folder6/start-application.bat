@echo off
title EchoBooster Application Launcher

REM EchoBooster Application Launcher
REM Version 4.1 - Modern UI with Node.js Integration
REM This script cleans up the project and starts the application

echo =============================================
echo    EchoBooster v4.1 - Application Launcher
echo =============================================
echo.

REM Change to the workspace directory
cd /d "%~dp0"

REM Clean up previous build artifacts
echo Cleaning up previous build artifacts...
if exist "EchoBooster\bin" rmdir /s /q "EchoBooster\bin" 2>nul
if exist "EchoBooster\obj" rmdir /s /q "EchoBooster\obj" 2>nul
if exist "start-echo.exe" del "start-echo.exe" 2>nul

REM Install Node.js dependencies if not already installed
echo Installing Node.js dependencies...
npm install --silent

REM Build the .NET application
echo Building .NET application...
dotnet build "EchoBooster\EchoBooster.csproj" --configuration Release

if errorlevel 1 (
    echo Build failed. Press any key to exit...
    pause >nul
    exit /b 1
)

REM Start the Node.js server in the background
echo Starting Node.js server...
start /min "EchoBooster Node Server" cmd /c "node server.js"

REM Wait a moment for the server to start
timeout /t 2 /nobreak >nul

REM Start the main application
echo Launching EchoBooster application...
start /wait "EchoBooster" dotnet run --project "EchoBooster\EchoBooster.csproj"

REM Cleanup on exit
echo.
echo Application closed. Cleaning up...
taskkill /f /im "node" 2>nul
taskkill /f /im "dotnet" 2>nul

echo.
echo Thank you for using EchoBooster v4.1!
pause