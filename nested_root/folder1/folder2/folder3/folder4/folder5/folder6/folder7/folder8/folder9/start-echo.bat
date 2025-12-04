@echo off
title EchoBooster Launcher

echo Starting EchoBooster Application...
echo.

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Try to find the EchoBooster executable
set "EXE_PATH="

if exist "%SCRIPT_DIR%EchoBooster.exe" set "EXE_PATH=%SCRIPT_DIR%EchoBooster.exe"
if exist "%SCRIPT_DIR%EchoBooster\bin\Release\net6.0-windows\EchoBooster.exe" set "EXE_PATH=%SCRIPT_DIR%EchoBooster\bin\Release\net6.0-windows\EchoBooster.exe"
if exist "%SCRIPT_DIR\EchoBooster\bin\Debug\net6.0-windows\EchoBooster.exe" set "EXE_PATH=%SCRIPT_DIR\EchoBooster\bin\Debug\net6.0-windows\EchoBooster.exe"
if exist "EchoBooster.exe" set "EXE_PATH=EchoBooster.exe"
if exist "..\EchoBooster.exe" set "EXE_PATH=..\EchoBooster.exe"

REM If executable is found, launch it
if not "%EXE_PATH%"=="" (
    echo Found EchoBooster at: %EXE_PATH%
    echo Launching EchoBooster...
    start "" "%EXE_PATH%"
    echo EchoBooster launched successfully!
) else (
    echo EchoBooster executable not found!
    echo Attempting to build and run from source...
    
    REM Check if the project exists and run it
    if exist "%SCRIPT_DIR%EchoBooster\EchoBooster.csproj" (
        echo Found EchoBooster project, building and running...
        cmd /k "cd /d "%SCRIPT_DIR%" && dotnet run --project EchoBooster"
    ) else (
        echo EchoBooster project not found. Please ensure the application is built first.
        pause
    )
)

echo.
echo Press any key to exit...
pause >nul