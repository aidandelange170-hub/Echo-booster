/**
 * EchoBooster Node.js Launcher with Enhanced Performance
 * This script launches both the Node.js server and the C# WPF application
 */

const { spawn, exec } = require('child_process');
const { platform } = require('os');
const { join, dirname } = require('path');
const fs = require('fs');
const http = require('http');

// Get the directory where this script is located
const scriptDir = __dirname;

/**
 * Check if a service is running on a given port
 */
function isPortInUse(port) {
    return new Promise((resolve) => {
        const client = new http.ClientRequest({
            hostname: 'localhost',
            port: port,
            path: '/',
            method: 'GET'
        });

        client.setTimeout(1000);
        client.on('error', () => resolve(false));
        client.on('response', () => resolve(true));
        client.end();
    });
}

/**
 * Find the EchoBooster executable in common locations
 */
function findEchoBoosterExecutable() {
    const possiblePaths = [
        // Current directory
        join(scriptDir, 'EchoBooster.exe'),
        join(scriptDir, 'EchoBooster'),
        
        // Subdirectories
        join(scriptDir, 'EchoBooster', 'bin', 'Release', 'net8.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'EchoBooster', 'bin', 'Debug', 'net8.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'EchoBooster', 'bin', 'Release', 'net8.0', 'EchoBooster'),
        join(scriptDir, 'EchoBooster', 'bin', 'Debug', 'net8.0', 'EchoBooster'),
        
        // Parent directory
        join(scriptDir, '..', 'EchoBooster.exe'),
        join(scriptDir, '..', 'EchoBooster'),
        
        // Common build locations
        join(scriptDir, 'bin', 'Release', 'net8.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'bin', 'Debug', 'net8.0-windows', 'EchoBooster.exe'),
        join(scriptDir, 'bin', 'Release', 'net8.0', 'EchoBooster'),
        join(scriptDir, 'bin', 'Debug', 'net8.0', 'EchoBooster'),
    ];

    for (const path of possiblePaths) {
        if (fs.existsSync(path)) {
            return path;
        }
    }

    return null;
}

/**
 * Start the Node.js server
 */
async function startNodeServer() {
    return new Promise((resolve) => {
        console.log('Starting EchoBooster Node.js server...');
        
        const serverProcess = spawn('node', ['server.js'], {
            cwd: scriptDir,
            stdio: ['pipe', 'pipe', 'pipe']
        });

        serverProcess.stdout.on('data', (data) => {
            const output = data.toString();
            console.log(`Server: ${output.trim()}`);
            
            // Wait for server to be ready
            if (output.includes('running on port')) {
                setTimeout(() => resolve(serverProcess), 2000); // Wait 2 seconds for full initialization
            }
        });

        serverProcess.stderr.on('data', (data) => {
            console.error(`Server Error: ${data}`);
        });

        serverProcess.on('close', (code) => {
            console.log(`Server process exited with code ${code}`);
        });
    });
}

/**
 * Open a terminal and run the EchoBooster application
 */
async function openTerminalAndRun() {
    console.log('Starting EchoBooster Application...');

    // Start Node.js server first
    const serverProcess = await startNodeServer();
    console.log('Node.js server started successfully!');

    // Try to find the executable
    const exePath = findEchoBoosterExecutable();

    if (exePath) {
        console.log(`Found EchoBooster at: ${exePath}`);
        console.log('Launching EchoBooster C# application...');

        try {
            if (platform() === 'win32') {
                // On Windows
                spawn(exePath, [], { 
                    detached: true, 
                    stdio: 'ignore',
                    env: { ...process.env, NODE_SERVER_STARTED: 'true' }
                });
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
            return { serverProcess, success: true };
        } catch (error) {
            console.log(`Error launching EchoBooster: ${error.message}`);
            return { serverProcess, success: false };
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
                        stdio: 'inherit',
                        env: { ...process.env, NODE_SERVER_STARTED: 'true' }
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
                    return { serverProcess, success: false };
                }
            }

            console.log('Terminal opened. Please navigate to the EchoBooster directory and run \\'dotnet run\\'.');
            return { serverProcess, success: true };
        } catch (error) {
            console.log(`Error opening terminal: ${error.message}`);
            return { serverProcess, success: false };
        }
    }
}

/**
 * Main function
 */
async function main() {
    console.log('EchoBooster Enhanced Launcher');
    console.log('='.repeat(30));
    console.log('Starting Node.js server and C# application...');

    const { serverProcess, success } = await openTerminalAndRun();

    if (success) {
        console.log('\\nBoth applications started successfully!');
        console.log('Node.js server is running with real-time monitoring capabilities.');
        console.log('C# WPF application provides the main UI experience.');
    } else {
        console.log('\\nOperation failed.');
    }

    // Keep the launcher running
    process.on('SIGINT', () => {
        console.log('\\nShutting down EchoBooster...');
        if (serverProcess) {
            serverProcess.kill();
        }
        process.exit(0);
    });

    // Wait for user input before exiting
    process.stdin.setEncoding('utf8');
    process.stdin.on('readable', () => {
        const chunk = process.stdin.read();
        if (chunk !== null) {
            if (serverProcess) {
                serverProcess.kill();
            }
            process.exit(0);
        }
    });

    console.log('\\nPress Enter to stop all services and exit...');
}

// Run the main function
main().catch(console.error);