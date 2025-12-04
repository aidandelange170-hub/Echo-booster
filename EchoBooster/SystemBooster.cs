using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EchoBooster
{
    public class SystemBooster
    {
        private bool isMonitoring = false;
        private Task? monitoringTask;
        private ProcessManager _processManager;
        
        public SystemBooster()
        {
            _processManager = new ProcessManager();
        }

        public void StartMonitoring()
        {
            if (!isMonitoring)
            {
                isMonitoring = true;
                monitoringTask = Task.Run(MonitoringLoop);
            }
        }
        
        private async Task MonitoringLoop()
        {
            while (isMonitoring)
            {
                // Perform periodic monitoring tasks
                await Task.Delay(5000); // Check every 5 seconds
            }
        }
        
        public SystemMetrics GetSystemMetrics()
        {
            return new SystemMetrics
            {
                CpuUsage = GetCpuUsage(),
                MemoryUsage = GetMemoryUsage(),
                DiskUsage = GetDiskUsage(),
                NetworkStatus = GetNetworkInfo(),
                TotalMemory = GetTotalMemory(),
                AvailableMemory = GetAvailableMemory(),
                ProcessCount = Process.GetProcesses().Length,
                ThreadCount = GetTotalThreadCount(),
                BootTime = GetBootTime()
            };
        }
        
        public void CheckPerformance()
        {
            var metrics = GetSystemMetrics();
            
            Console.WriteLine("\n--- System Performance Report ---");
            Console.WriteLine($"CPU Usage: {metrics.CpuUsage:F2}%");
            Console.WriteLine($"Memory Usage: {metrics.MemoryUsage:F2}% ({metrics.AvailableMemory} MB free of {metrics.TotalMemory} MB)");
            Console.WriteLine($"Disk Usage: {metrics.DiskUsage:F2}%");
            Console.WriteLine($"Network Status: {metrics.NetworkStatus.status}");
            Console.WriteLine($"Running Processes: {metrics.ProcessCount}");
            Console.WriteLine($"Total Threads: {metrics.ThreadCount}");
        }
        
        public void OptimizeProcesses()
        {
            Console.WriteLine("\n--- Process Optimization ---");
            Console.WriteLine("Optimizing system processes...");
            
            try
            {
                // Get all running processes
                var processes = Process.GetProcesses();
                
                int optimizedCount = 0;
                
                foreach (var process in processes)
                {
                    try
                    {
                        // Skip system critical processes
                        if (IsSystemProcess(process.ProcessName))
                            continue;
                            
                        // Set process priority to normal if it's too high or too low
                        if (process.BasePriority != ProcessPriorityClass.Normal)
                        {
                            // Only change priority for non-system processes
                            process.PriorityClass = ProcessPriorityClass.Normal;
                            optimizedCount++;
                        }
                        
                        // Optimize process memory by calling GC
                        process.Refresh();
                    }
                    catch
                    {
                        // Ignore processes that can't be accessed
                    }
                }
                
                // Clean up system memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                
                // On Windows, we can try to reduce working set size
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ReduceWorkingSet();
                }
                
                Console.WriteLine($"Optimized {optimizedCount} processes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error optimizing processes: {ex.Message}");
            }
        }
        
        public void MonitorNetwork()
        {
            Console.WriteLine("\n--- Network Performance Monitor ---");
            
            var networkInfo = GetNetworkInfo();
            Console.WriteLine($"Network Status: {networkInfo.status}");
            Console.WriteLine($"Network Speed: {networkInfo.speed}");
            Console.WriteLine($"Active Connections: {networkInfo.connections}");
        }
        
        private double GetCpuUsage()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var startCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
                
                // Wait for a short period to calculate CPU usage
                Task.Delay(500).Wait();
                
                var endTime = DateTime.UtcNow;
                var endCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
                
                var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                
                var cpuUsage = (cpuUsedMs / (Environment.ProcessorCount * totalMsPassed)) * 100;
                
                return Math.Min(100, Math.Max(0, cpuUsage));
            }
            catch
            {
                return 0.0;
            }
        }
        
        private double GetMemoryUsage()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Use PerformanceCounter to get actual system memory usage
                    using (var pc = new PerformanceCounter("Memory", "Available MBytes"))
                    {
                        pc.NextValue(); // Call first to get initial value
                        System.Threading.Thread.Sleep(100); // Wait a bit
                        float availableMemory = pc.NextValue();
                        
                        // Get total memory
                        var totalMemory = GetTotalMemory();
                        
                        // Calculate usage percentage
                        var usedMemory = totalMemory - availableMemory;
                        var percentage = (usedMemory / totalMemory) * 100;
                        
                        return Math.Min(100, Math.Max(0, percentage));
                    }
                }
                else
                {
                    // For non-Windows platforms, return a simulated value
                    // In a real application, you would use platform-specific methods
                    return 30.0; // Simulated value
                }
            }
            catch
            {
                return 0.0;
            }
        }
        
        private float GetTotalMemory()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var totalMemoryKb = Convert.ToInt64(obj["TotalVisibleMemorySize"]);
                            obj.Dispose();
                            return totalMemoryKb / 1024.0f; // Convert to MB
                        }
                    }
                }
            }
            catch
            {
                // Fallback to a reasonable default
            }
            
            // Return a default value if we can't get the actual value
            return 8192.0f; // 8GB in MB
        }
        
        private float GetAvailableMemory()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var freeMemoryKb = Convert.ToInt64(obj["FreePhysicalMemory"]);
                            obj.Dispose();
                            return freeMemoryKb / 1024.0f; // Convert to MB
                        }
                    }
                }
            }
            catch
            {
                // Fallback to a reasonable default
            }
            
            // Return a default value if we can't get the actual value
            return 4096.0f; // 4GB in MB
        }
        
        private double GetDiskUsage()
        {
            try
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed);
                if (drive != null)
                {
                    var totalSize = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    var availableFreeSpace = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    var usedSpace = totalSize - availableFreeSpace;
                    var percentage = (usedSpace / totalSize) * 100;
                    
                    return Math.Min(100, Math.Max(0, percentage));
                }
            }
            catch
            {
                // Handle exception
            }
            
            return 0.0;
        }
        
        private (string status, string speed, int connections) GetNetworkInfo()
        {
            try
            {
                var status = "Disconnected";
                var connections = 0;
                
                // Count active network connections
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnections = properties.GetActiveTcpConnections();
                var tcpListeners = properties.GetActiveTcpListeners();
                var udpListeners = properties.GetActiveUdpListeners();
                
                connections = tcpConnections.Length + tcpListeners.Length + udpListeners.Length;
                
                // Check if any network interface is up
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                var activeInterface = interfaces.FirstOrDefault(i => i.OperationalStatus == OperationalStatus.Up && 
                    i.NetworkInterfaceType != NetworkInterfaceType.Loopback);
                
                if (activeInterface != null)
                {
                    status = "Connected";
                }
                
                // For network speed, we'll return a placeholder
                // In a real application, you would measure actual network throughput
                var speed = activeInterface?.Speed > 0 ? $"{activeInterface.Speed / 1_000_000} Mbps" : "Unknown";
                
                return (status, speed, connections);
            }
            catch
            {
                return ("Unknown", "Unknown", 0);
            }
        }
        
        private int GetTotalThreadCount()
        {
            try
            {
                var processes = Process.GetProcesses();
                int totalThreads = 0;
                
                foreach (var process in processes)
                {
                    try
                    {
                        totalThreads += process.Threads.Count;
                    }
                    catch
                    {
                        // Ignore processes that can't be accessed
                    }
                }
                
                return totalThreads;
            }
            catch
            {
                return 0;
            }
        }
        
        private DateTime GetBootTime()
        {
            try
            {
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                return DateTime.Now.Subtract(uptime);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);
        
        private void ReduceWorkingSet()
        {
            try
            {
                EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            }
            catch
            {
                // This is a Windows-specific optimization that may not work in all contexts
            }
        }
        
        private bool IsSystemProcess(string processName)
        {
            // List of system processes that should not be modified
            string[] systemProcesses = {
                "System", "svchost", "explorer", "winlogon", "csrss",
                "lsass", "services", "wininit", "smss", "System Idle Process",
                "winmgmt", "spoolsv", "taskhost", "dwm", "ctfmon"
            };
            
            return systemProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase);
        }
        
        public void StopMonitoring()
        {
            isMonitoring = false;
            monitoringTask?.Wait(1000); // Wait up to 1 second for task to finish
            _processManager?.Dispose();
        }

        public List<ProcessManager.ProcessInfo> GetProcessList()
        {
            return _processManager.GetProcessesWithUsage();
        }

        public async Task CloseBackgroundProcesses()
        {
            await _processManager.CloseBackgroundProcesses();
        }

        public string GetActiveApplicationName()
        {
            return _processManager.GetActiveApplicationName();
        }
    }
    
    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public (string status, string speed, int connections) NetworkStatus { get; set; }
        public float TotalMemory { get; set; }
        public float AvailableMemory { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
        public DateTime BootTime { get; set; }
    }
}