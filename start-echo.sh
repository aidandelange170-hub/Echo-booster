#!/bin/bash

# Shell script to launch EchoBooster application
# This script opens a terminal and runs the EchoBooster application

echo "Starting EchoBooster Application..."

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Try to find the EchoBooster executable
POSSIBLE_PATHS=(
    "$SCRIPT_DIR/EchoBooster"
    "$SCRIPT_DIR/EchoBooster.exe"
    "$SCRIPT_DIR/EchoBooster/bin/Release/net6.0/EchoBooster"
    "$SCRIPT_DIR/EchoBooster/bin/Debug/net6.0/EchoBooster"
    "$(pwd)/EchoBooster"
    "$(pwd)/EchoBooster.exe"
)

ECHOBOOSTER_PATH=""

for path in "${POSSIBLE_PATHS[@]}"; do
    if [ -f "$path" ]; then
        ECHOBOOSTER_PATH="$path"
        break
    fi
done

if [ -n "$ECHOBOOSTER_PATH" ]; then
    echo "Found EchoBooster at: $ECHOBOOSTER_PATH"
    echo "Launching EchoBooster..."
    
    # Start the application
    if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
        # On Windows with MSYS/Cygwin
        start "" "$ECHOBOOSTER_PATH"
    else
        # On Linux/macOS, use appropriate method
        if command -v xdg-open &> /dev/null; then
            xdg-open "$ECHOBOOSTER_PATH"
        elif command -v open &> /dev/null; then
            open "$ECHOBOOSTER_PATH"
        else
            "$ECHOBOOSTER_PATH" &
        fi
    fi
    
    echo "EchoBooster launched successfully!"
else
    echo "EchoBooster executable not found!"
    echo "Attempting to build and run from source..."
    
    # Try to run from source if executable doesn't exist
    if [ -f "$SCRIPT_DIR/EchoBooster/EchoBooster.csproj" ]; then
        echo "Found EchoBooster project, building and running..."
        cd "$SCRIPT_DIR"
        if command -v gnome-terminal &> /dev/null; then
            gnome-terminal -- bash -c "cd '$SCRIPT_DIR'; dotnet run --project EchoBooster; exec bash"
        elif command -v osascript &> /dev/null; then
            # macOS
            osascript -e "tell app \"Terminal\" to do script \"cd '$SCRIPT_DIR'; dotnet run --project EchoBooster\""
        else
            # Fallback to regular terminal
            (cd "$SCRIPT_DIR" && dotnet run --project EchoBooster) &
        fi
    else
        echo "EchoBooster project not found. Please ensure the application is built first."
    fi
fi

echo
read -p "Press Enter to exit..."