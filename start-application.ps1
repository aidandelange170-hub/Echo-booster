# EchoBooster Application Launcher
# Version 4.1 - Modern UI with Node.js Integration
# This script cleans up the project and starts the application

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "    EchoBooster v4.1 - Application Launcher" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Change to the workspace directory
Set-Location $PSScriptRoot

# Clean up previous build artifacts
Write-Host "Cleaning up previous build artifacts..." -ForegroundColor Yellow
if (Test-Path "EchoBooster\bin") {
    Remove-Item "EchoBooster\bin" -Recurse -Force
}
if (Test-Path "EchoBooster\obj") {
    Remove-Item "EchoBooster\obj" -Recurse -Force
}

# Install Node.js dependencies if not already installed
Write-Host "Installing Node.js dependencies..." -ForegroundColor Yellow
npm install --silent

# Build the .NET application
Write-Host "Building .NET application..." -ForegroundColor Yellow
dotnet build "EchoBooster\EchoBooster.csproj" --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    exit 1
}

# Start the Node.js server in the background
Write-Host "Starting Node.js server..." -ForegroundColor Yellow
Start-Process node -ArgumentList "server.js" -WindowStyle Hidden

# Wait a moment for the server to start
Start-Sleep -Seconds 2

# Start the main application
Write-Host "Launching EchoBooster application..." -ForegroundColor Green
dotnet run --project "EchoBooster\EchoBooster.csproj"

# Cleanup on exit
Write-Host ""
Write-Host "Application closed. Cleaning up..." -ForegroundColor Yellow

Write-Host ""
Write-Host "Thank you for using EchoBooster v4.1!" -ForegroundColor Green