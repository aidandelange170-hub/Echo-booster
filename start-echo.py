#!/usr/bin/env python3
"""
Python script to launch EchoBooster application
This script opens a terminal and runs the EchoBooster application
"""

import os
import sys
import subprocess
import platform
from pathlib import Path


def find_echo_booster_executable():
    """Find the EchoBooster executable in common locations"""
    possible_paths = [
        # Current directory
        "./EchoBooster.exe",
        "./EchoBooster",
        
        # Subdirectories
        "./EchoBooster/bin/Release/net6.0-windows/EchoBooster.exe",
        "./EchoBooster/bin/Debug/net6.0-windows/EchoBooster.exe",
        "./EchoBooster/bin/Release/net6.0/EchoBooster",
        "./EchoBooster/bin/Debug/net6.0/EchoBooster",
        
        # Parent directory
        "../EchoBooster.exe",
        "../EchoBooster",
        
        # Common build locations
        "./bin/Release/net6.0-windows/EchoBooster.exe",
        "./bin/Debug/net6.0-windows/EchoBooster.exe",
        "./bin/Release/net6.0/EchoBooster",
        "./bin/Debug/net6.0/EchoBooster",
    ]
    
    script_dir = Path(__file__).parent.absolute()
    
    for path in possible_paths:
        full_path = script_dir / path
        if full_path.exists():
            return str(full_path)
    
    return None


def open_terminal_and_run():
    """Open a terminal and run the EchoBooster application"""
    print("Starting EchoBooster Application...")
    
    # Try to find the executable
    exe_path = find_echo_booster_executable()
    
    if exe_path:
        print(f"Found EchoBooster at: {exe_path}")
        print("Launching EchoBooster...")
        
        try:
            if platform.system() == "Windows":
                # On Windows
                subprocess.Popen([exe_path], shell=True)
            elif platform.system() == "Darwin":  # macOS
                # On macOS, try to open in Terminal
                subprocess.Popen(['open', '-a', 'Terminal', exe_path])
            else:  # Linux
                # On Linux, try different terminal emulators
                terminals = ['gnome-terminal', 'konsole', 'xfce4-terminal', 'xterm']
                terminal_found = False
                
                for terminal in terminals:
                    if subprocess.run(['which', terminal], capture_output=True).returncode == 0:
                        subprocess.Popen([terminal, '-e', exe_path])
                        terminal_found = True
                        break
                
                if not terminal_found:
                    # Fallback: run directly
                    subprocess.Popen([exe_path])
            
            print("EchoBooster launched successfully!")
        except Exception as e:
            print(f"Error launching EchoBooster: {e}")
            return False
    else:
        print("EchoBooster executable not found!")
        print("Attempting to build and run from source...")
        
        # Try to run from source using dotnet if available
        try:
            result = subprocess.run(['dotnet', '--version'], capture_output=True, text=True)
            if result.returncode == 0:
                # DotNet is available, try to run the project
                script_dir = Path(__file__).parent.absolute()
                project_path = script_dir / "EchoBooster" / "EchoBooster.csproj"
                
                if project_path.exists():
                    print("Found EchoBooster project, building and running...")
                    subprocess.Popen(['dotnet', 'run', '--project', 'EchoBooster'], cwd=script_dir)
                    return True
                else:
                    print("EchoBooster project not found.")
            else:
                print("DotNet SDK not found. Please install .NET SDK to run from source.")
        except FileNotFoundError:
            print("DotNet SDK not found. Please install .NET SDK to run from source.")
        
        # Try to open a terminal for manual operation
        try:
            if platform.system() == "Windows":
                subprocess.Popen(['cmd'], shell=True)
            elif platform.system() == "Darwin":
                subprocess.Popen(['open', '-a', 'Terminal', '.'])
            else:  # Linux
                terminals = ['gnome-terminal', 'konsole', 'xfce4-terminal', 'xterm']
                terminal_found = False
                
                for terminal in terminals:
                    if subprocess.run(['which', terminal], capture_output=True).returncode == 0:
                        subprocess.Popen([terminal])
                        terminal_found = True
                        break
                
                if not terminal_found:
                    print("No known terminal found. Please open a terminal manually.")
                    return False
            
            print("Terminal opened. Please navigate to the EchoBooster directory and run 'dotnet run'.")
            return True
        except Exception as e:
            print(f"Error opening terminal: {e}")
            return False
    
    return True


def main():
    """Main function"""
    print("EchoBooster Launcher")
    print("=" * 20)
    
    success = open_terminal_and_run()
    
    if success:
        print("\nOperation completed successfully.")
    else:
        print("\nOperation failed.")
    
    # Wait for user input before exiting
    input("\nPress Enter to exit...")


if __name__ == "__main__":
    main()