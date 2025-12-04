# PowerShell script to launch EchoBooster application
# This script opens a terminal and runs the EchoBooster application

Write-Host "Starting EchoBooster Application..." -ForegroundColor Green

# Get the directory where this script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Try to find the EchoBooster executable
$PossiblePaths = @(
    "$ScriptDir\EchoBooster.exe",
    "$ScriptDir\EchoBooster\bin\Release\net6.0-windows\EchoBooster.exe",
    "$ScriptDir\EchoBooster\bin\Debug\net6.0-windows\EchoBooster.exe",
    "$PWD\EchoBooster.exe",
    "$PWD\EchoBooster\EchoBooster.exe",
    "$env:USERPROFILE\Downloads\EchoBooster.exe"
)

$EchoBoosterPath = $null
foreach ($Path in $PossiblePaths) {
    if (Test-Path $Path) {
        $EchoBoosterPath = $Path
        break
    }
}

if ($EchoBoosterPath) {
    Write-Host "Found EchoBooster at: $EchoBoosterPath" -ForegroundColor Yellow
    Write-Host "Launching EchoBooster..." -ForegroundColor Green
    
    # Start the application
    Start-Process -FilePath $EchoBoosterPath
    
    Write-Host "EchoBooster launched successfully!" -ForegroundColor Green
} else {
    Write-Host "EchoBooster executable not found!" -ForegroundColor Red
    Write-Host "Attempting to build and run from source..." -ForegroundColor Yellow
    
    # Try to run from source if executable doesn't exist
    $EchoBoosterProject = "$ScriptDir\EchoBooster\EchoBooster.csproj"
    if (Test-Path $EchoBoosterProject) {
        Write-Host "Found EchoBooster project, building and running..." -ForegroundColor Yellow
        Start-Process -FilePath "cmd.exe" -ArgumentList "/k", "cd /d `"$ScriptDir`" && dotnet run --project EchoBooster"
    } else {
        Write-Host "EchoBooster project not found. Please ensure the application is built first." -ForegroundColor Red
    }
}

# Keep the terminal open for a few seconds to show the message
Start-Sleep -Seconds 3