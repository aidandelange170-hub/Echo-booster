#!/bin/bash

# EchoBooster Application Launcher
# Version 4.1 - Modern UI with Node.js Integration
# This script cleans up the project and starts the application

echo "============================================="
echo "    EchoBooster v4.1 - Application Launcher"
echo "============================================="
echo

# Change to the workspace directory
cd "$(dirname "$0")"

# Clean up previous build artifacts
echo "Cleaning up previous build artifacts..."
rm -rf "EchoBooster/bin" "EchoBooster/obj" 2>/dev/null || true

# Install Node.js dependencies if not already installed
echo "Installing Node.js dependencies..."
npm install --silent

# Build the .NET application
echo "Building .NET application..."
dotnet build "EchoBooster/EchoBooster.csproj" --configuration Release

if [ $? -ne 0 ]; then
    echo "Build failed."
    exit 1
fi

# Start the Node.js server in the background
echo "Starting Node.js server..."
node server.js > node.log 2>&1 &
NODE_PID=$!

# Wait a moment for the server to start
sleep 2

# Start the main application
echo "Launching EchoBooster application..."
dotnet run --project "EchoBooster/EchoBooster.csproj"

# Cleanup on exit
echo
echo "Application closed. Cleaning up..."
kill $NODE_PID 2>/dev/null || true

echo
echo "Thank you for using EchoBooster v4.1!"