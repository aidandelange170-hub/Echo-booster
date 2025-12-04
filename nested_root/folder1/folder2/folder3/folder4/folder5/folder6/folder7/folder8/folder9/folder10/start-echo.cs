using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace EchoLauncher
{
    /// <summary>
    /// A DLL that can launch the EchoBooster application via terminal
    /// </summary>
    public class StartEcho
    {
        public static void LaunchApplication()
        {
            try
            {
                // Get the directory where this DLL is located
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string baseDirectory = Path.GetDirectoryName(assemblyLocation);
                
                // Try to find the EchoBooster executable
                string exePath = FindEchoBoosterExecutable(baseDirectory);
                
                if (string.IsNullOrEmpty(exePath))
                {
                    Console.WriteLine("EchoBooster executable not found!");
                    return;
                }
                
                // Start a new process to run the application
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" \"{exePath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                
                Process.Start(startInfo);
                Console.WriteLine("EchoBooster launched successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error launching EchoBooster: {ex.Message}");
            }
        }
        
        private static string FindEchoBoosterExecutable(string searchDirectory)
        {
            // Look for EchoBooster executable in common locations
            string[] possiblePaths = {
                Path.Combine(searchDirectory, "EchoBooster.exe"),
                Path.Combine(searchDirectory, "EchoBooster", "bin", "Release", "net6.0-windows", "EchoBooster.exe"),
                Path.Combine(searchDirectory, "EchoBooster", "bin", "Debug", "net6.0-windows", "EchoBooster.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "EchoBooster.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "EchoBooster", "bin", "Release", "net6.0-windows", "EchoBooster.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "EchoBooster", "bin", "Debug", "net6.0-windows", "EchoBooster.exe")
            };
            
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        // Method to open terminal and run the application
        public static void OpenTerminalAndRun()
        {
            try
            {
                // Open command prompt/terminal
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/k echo EchoBooster is starting... && dotnet run --project EchoBooster",
                    UseShellExecute = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };
                
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening terminal: {ex.Message}");
                
                // Fallback: try to run the application directly
                LaunchApplication();
            }
        }
    }
}