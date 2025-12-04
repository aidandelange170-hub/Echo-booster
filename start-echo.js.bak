/**
 * Node.js script to launch EchoBooster application
 * This script opens a terminal and runs the EchoBooster application
 */

const { spawn, exec } = require('child_process');
const { platform } = require('os');
const { join, dirname } = require('path');
const fs = require('fs');

// Get the directory where this script is located
const scriptDir = __dirname;

/**
 * Find the EchoBooster executable in common locations
 */
function findEchoBoosterExecutable() {
    const possiblePaths = [
        // Current directory
        join(scriptDir, 'EchoBooster.exe'),
        join(scriptDir, 'EchoBooster'),
        
        // Subdirectories
        join(scriptDir, 'EchoBooster', 'bin', 'Release', 'net6.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'EchoBooster', 'bin', 'Debug', 'net6.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'EchoBooster', 'bin', 'Release', 'net6.0', 'EchoBooster'),
        join(scriptDir, 'EchoBooster', 'bin', 'Debug', 'net6.0', 'EchoBooster'),
        
        // Parent directory
        join(scriptDir, '..', 'EchoBooster.exe'),
        join(scriptDir, '..', 'EchoBooster'),
        
        // Common build locations
        join(scriptDir, 'bin', 'Release', 'net6.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'bin', 'Debug', 'net6.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'bin', 'Release', 'net6.0', 'EchoBooster'),
        join(scriptDir, 'bin', 'Debug', 'net6.0', 'EchoBooster'),
    ];

    for (const path of possiblePaths) {
        if (fs.existsSync(path)) {
            return path;
        }
    }

    return null;
}

/**
 * Open a terminal and run the EchoBooster application
 */
function openTerminalAndRun() {
    console.log('Starting EchoBooster Application...');

    // Try to find the executable
    const exePath = findEchoBoosterExecutable();

    if (exePath) {
        console.log(`Found EchoBooster at: ${exePath}`);
        console.log('Launching EchoBooster...');

        try {
            if (platform() === 'win32') {
                // On Windows
                spawn(exePath, [], { detached: true, stdio: 'ignore' });
            } else if (platform() === 'darwin') {  // macOS
                // On macOS, try to open in Terminal
                spawn('open', ['-a', 'Terminal', exePath]);
            } else {  // Linux
                // On Linux, try different terminal emulators
                const terminals = ['gnome-terminal', 'konsole', 'xfce4-terminal', 'xterm'];
                let terminalFound = false;

                for (const terminal of terminals) {
                    exec(`which ${terminal}`, (error, stdout, stderr) => {
                        if (!error && stdout.trim()) {
                            spawn(terminal, ['-e', exePath]);
                            terminalFound = true;
                            return;
                        }
                    });
                }

                if (!terminalFound) {
                    // Fallback: run directly
                    spawn(exePath);
                }
            }

            console.log('EchoBooster launched successfully!');
            return true;
        } catch (error) {
            console.log(`Error launching EchoBooster: ${error.message}`);
            return false;
        }
    } else {
        console.log('EchoBooster executable not found!');
        console.log('Attempting to build and run from source...');

        // Try to run from source using dotnet if available
        exec('dotnet --version', (error, stdout, stderr) => {
            if (!error) {
                // DotNet is available, try to run the project
                const projectPath = join(scriptDir, 'EchoBooster', 'EchoBooster.csproj');

                if (fs.existsSync(projectPath)) {
                    console.log('Found EchoBooster project, building and running...');
                    spawn('dotnet', ['run', '--project', 'EchoBooster'], {
                        cwd: scriptDir,
                        stdio: 'inherit'
                    });
                } else {
                    console.log('EchoBooster project not found.');
                }
            } else {
                console.log('DotNet SDK not found. Please install .NET SDK to run from source.');
            }
        });

        // Try to open a terminal for manual operation
        try {
            if (platform() === 'win32') {
                spawn('cmd', [], { detached: true, stdio: 'ignore' });
            } else if (platform() === 'darwin') {  // macOS
                spawn('open', ['-a', 'Terminal', scriptDir]);
            } else {  // Linux
                const terminals = ['gnome-terminal', 'konsole', 'xfce4-terminal', 'xterm'];
                let terminalFound = false;

                for (const terminal of terminals) {
                    exec(`which ${terminal}`, (error, stdout, stderr) => {
                        if (!error && stdout.trim()) {
                            spawn(terminal, [], { detached: true, stdio: 'ignore' });
                            terminalFound = true;
                            return;
                        }
                    });
                }

                if (!terminalFound) {
                    console.log('No known terminal found. Please open a terminal manually.');
                    return false;
                }
            }

            console.log('Terminal opened. Please navigate to the EchoBooster directory and run \'dotnet run\'.');
            return true;
        } catch (error) {
            console.log(`Error opening terminal: ${error.message}`);
            return false;
        }
    }

    return true;
}

/**
 * Main function
 */
function main() {
    console.log('EchoBooster Launcher');
    console.log('='.repeat(20));

    const success = openTerminalAndRun();

    if (success) {
        console.log('\nOperation completed successfully.');
    } else {
        console.log('\nOperation failed.');
    }

    // Wait for user input before exiting (in a real implementation, you might want to handle this differently)
    process.stdin.setEncoding('utf8');
    process.stdin.on('readable', () => {
        const chunk = process.stdin.read();
        if (chunk !== null) {
            process.exit(0);
        }
    });

    console.log('\nPress Enter to exit...');
}

// Run the main function
main();