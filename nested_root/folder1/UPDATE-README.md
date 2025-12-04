# EchoBooster Application Update

## Overview
This update includes multiple file types to launch the EchoBooster application with various methods, fixing bugs and adding new features.

## File Types Included

### 1. DLL File: `start-echo.dll`
- A .NET library that can launch the EchoBooster application
- Contains methods to open terminal and run the application without manual steps
- Provides programmatic access to launch functionality

### 2. Batch File: `start-echo.bat`
- Windows batch script for easy launching
- Automatically finds and runs the EchoBooster executable
- Includes fallback to build and run from source if executable is missing

### 3. PowerShell Script: `start-echo.ps1`
- PowerShell script with enhanced error handling
- Cross-platform compatibility features
- Detailed logging and feedback

### 4. Shell Script: `start-echo.sh`
- Bash script for Linux/macOS compatibility
- Automatic platform detection and appropriate launch method
- Terminal opening capabilities

## New Features Added

1. **Automatic Application Detection**: Scripts automatically locate the EchoBooster executable in common build locations
2. **Terminal Integration**: Direct terminal opening with application launch
3. **Cross-Platform Support**: Scripts work on Windows, Linux, and macOS
4. **Error Handling**: Comprehensive error handling and fallback mechanisms
5. **Build Integration**: Automatic build and run from source if executable is not found

## Bug Fixes

1. Fixed path resolution issues
2. Resolved executable detection problems
3. Corrected cross-platform compatibility issues
4. Improved error reporting and user feedback
5. Enhanced security by validating file paths

## How to Use

### Method 1: Direct DLL Usage
```csharp
using EchoLauncher;

StartEcho.LaunchApplication(); // Launches the application directly
StartEcho.OpenTerminalAndRun(); // Opens terminal and runs the application
```

### Method 2: Batch File
Double-click `start-echo.bat` or run from command line:
```
start-echo.bat
```

### Method 3: PowerShell
Run from PowerShell:
```
.\start-echo.ps1
```

### Method 4: Shell Script (Linux/macOS)
Make executable and run:
```bash
chmod +x start-echo.sh
./start-echo.sh
```

## Build Instructions

To build the DLL:
```bash
dotnet build start-echo.csproj
```

The compiled DLL will be in the `bin/` directory.

## Security Notice

All scripts and executables should be run from trusted sources. The PowerShell script may require execution policy changes on some systems:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Changelog

### Version 1.0
- Initial release with multiple launcher options
- Cross-platform compatibility
- Automatic executable detection
- Terminal integration
- Comprehensive error handling