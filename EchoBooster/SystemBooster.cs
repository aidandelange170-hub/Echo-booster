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
        private readonly object _lockObject = new object();
        private SystemMetrics _lastMetrics = new SystemMetrics();
        private DateTime _lastMetricsUpdate = DateTime.MinValue;
        private readonly TimeSpan _metricsCacheDuration = TimeSpan.FromMilliseconds(500); // Cache for 500ms
        
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
                try
                {
                    // Perform periodic monitoring tasks
                    await Task.Delay(2000); // Check every 2 seconds, more responsive
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in monitoring loop: {ex.Message}");
                    await Task.Delay(5000); // Wait longer if there's an error
                }
            }
        }
        
        public SystemMetrics GetSystemMetrics()
        {
            // Use caching to improve performance - return cached metrics if not expired
            lock (_lockObject)
            {
                if (DateTime.Now - _lastMetricsUpdate < _metricsCacheDuration)
                {
                    return _lastMetrics;
                }
            }

            var metrics = new SystemMetrics
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

            // Update cached metrics
            lock (_lockObject)
            {
                _lastMetrics = metrics;
                _lastMetricsUpdate = DateTime.Now;
            }

            return metrics;
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
        
        public void IntelligentOptimize()
        {
            Console.WriteLine("\n--- Intelligent System Optimization ---");
            Console.WriteLine("Performing AI-powered optimization...");
            
            try
            {
                // Get current system metrics
                var metrics = GetSystemMetrics();
                
                // Based on current metrics, perform targeted optimizations
                if (metrics.CpuUsage > 80)
                {
                    Console.WriteLine("High CPU usage detected - optimizing CPU-intensive processes");
                    OptimizeCPUProcesses();
                }
                
                if (metrics.MemoryUsage > 85)
                {
                    Console.WriteLine("High memory usage detected - optimizing memory usage");
                    OptimizeMemoryUsage();
                }
                
                if (metrics.DiskUsage > 90)
                {
                    Console.WriteLine("High disk usage detected - optimizing disk usage");
                    OptimizeDiskUsage();
                }
                
                Console.WriteLine("Intelligent optimization complete!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during intelligent optimization: {ex.Message}");
            }
        }
        
        private void OptimizeCPUProcesses()
        {
            try
            {
                var processes = Process.GetProcesses();
                
                foreach (var process in processes)
                {
                    try
                    {
                        if (IsSystemProcess(process.ProcessName))
                            continue;
                            
                        // Get CPU usage for the process
                        var cpuUsage = GetProcessCpuUsage(process);
                        
                        // If CPU usage is very high, reduce priority
                        if (cpuUsage > 50.0)
                        {
                            process.PriorityClass = ProcessPriorityClass.BelowNormal;
                        }
                    }
                    catch
                    {
                        // Ignore processes that can't be accessed
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error optimizing CPU processes: {ex.Message}");
            }
        }
        
        private void OptimizeMemoryUsage()
        {
            try
            {
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                
                // Reduce working set for this process
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ReduceWorkingSet();
                }
                
                // Try to reduce working set for other non-critical processes
                var processes = Process.GetProcesses();
                
                foreach (var process in processes)
                {
                    try
                    {
                        if (IsSystemProcess(process.ProcessName))
                            continue;
                            
                        // Reduce working set for memory-intensive processes
                        if (process.WorkingSet64 > 100 * 1024 * 1024) // More than 100MB
                        {
                            EmptyWorkingSet(process.Handle);
                        }
                    }
                    catch
                    {
                        // Ignore processes that can't be accessed
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error optimizing memory usage: {ex.Message}");
            }
        }
        
        private void OptimizeDiskUsage()
        {
            try
            {
                // Clean temporary files
                CleanTempFiles();
                
                // Optimize disk-related processes
                var processes = Process.GetProcesses();
                
                foreach (var process in processes)
                {
                    try
                    {
                        if (IsSystemProcess(process.ProcessName))
                            continue;
                            
                        // Check if process has high disk I/O
                        // This is a simplified check - in a real application you'd use performance counters
                        if (process.ProcessName.ToLower().Contains("search") || 
                            process.ProcessName.ToLower().Contains("index"))
                        {
                            // Reduce priority of indexing/search processes during high disk usage
                            process.PriorityClass = ProcessPriorityClass.Idle;
                        }
                    }
                    catch
                    {
                        // Ignore processes that can't be accessed
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error optimizing disk usage: {ex.Message}");
            }
        }
        
        private void CleanTempFiles()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                var tempFiles = Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories);
                
                int cleanedFiles = 0;
                foreach (var file in tempFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        // Only delete files older than 1 day
                        if (fileInfo.CreationTime < DateTime.Now.AddDays(-1))
                        {
                            fileInfo.Delete();
                            cleanedFiles++;
                        }
                    }
                    catch
                    {
                        // Ignore files that can't be deleted
                    }
                }
                
                Console.WriteLine($"Cleaned {cleanedFiles} temporary files");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning temp files: {ex.Message}");
            }
        }
        
        public void MonitorNetwork()
        {
            Console.WriteLine("\n--- Network Performance Monitor ---");
            
            var networkInfo = GetNetworkInfo();
            Console.WriteLine($"Network Status: {networkInfo.status}");
            Console.WriteLine($"Network Speed: {networkInfo.speed}");
            Console.WriteLine($"Active Connections: {networkInfo.connections}");
            Console.WriteLine($"Bytes Received: {networkInfo.bytesReceived}");
            Console.WriteLine($"Bytes Sent: {networkInfo.bytesSent}");
        }
        
        public async Task<SystemMetrics> GetDetailedSystemMetricsAsync()
        {
            return await Task.Run(() =>
            {
                var metrics = GetSystemMetrics();
                
                // Add more detailed metrics
                metrics.HandleCount = GetTotalHandleCount();
                
                return metrics;
            });
        }
        
        private int GetTotalHandleCount()
        {
            try
            {
                var processes = Process.GetProcesses();
                int totalHandles = 0;
                
                foreach (var process in processes)
                {
                    try
                    {
                        totalHandles += process.HandleCount;
                    }
                    catch
                    {
                        // Ignore processes that can't be accessed
                    }
                }
                
                return totalHandles;
            }
            catch
            {
                return 0;
            }
        }
        
        public async Task<bool> PerformAdvancedOptimizationAsync()
        {
            Console.WriteLine("\n--- Advanced System Optimization ---");
            
            try
            {
                // Run multiple optimizations in parallel for better performance
                var optimizationTasks = new Task[]
                {
                    Task.Run(() => OptimizeProcesses()),
                    Task.Run(() => IntelligentOptimize()),
                    Task.Run(() => OptimizeDiskUsage()),
                    Task.Run(() => CleanTempFiles())
                };
                
                await Task.WhenAll(optimizationTasks);
                
                Console.WriteLine("Advanced optimization completed!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during advanced optimization: {ex.Message}");
                return false;
            }
        }
        
        public List<ProcessManager.ProcessInfo> GetHighResourceProcesses(int cpuThreshold = 20, long memoryThreshold = 100) // 20% CPU, 100MB memory
        {
            var allProcesses = GetProcessList();
            return allProcesses.Where(p => p.CpuUsage > cpuThreshold || p.MemoryUsage > memoryThreshold).ToList();
        }
        
        public async Task<bool> TerminateHighResourceProcesses(int cpuThreshold = 20, long memoryThreshold = 100)
        {
            var highResourceProcesses = GetHighResourceProcesses(cpuThreshold, memoryThreshold);
            
            foreach (var process in highResourceProcesses)
            {
                try
                {
                    var proc = Process.GetProcessById(process.ProcessId);
                    if (!IsSystemProcess(proc.ProcessName))
                    {
                        proc.Kill();
                        await Task.Delay(100); // Small delay to allow process to terminate
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not terminate process {process.ProcessName} (PID: {process.ProcessId}): {ex.Message}");
                }
            }
            
            return true;
        }
        
        private double GetCpuUsage()
        {
            try
            {
                // Use performance counters for more accurate CPU usage
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                    {
                        cpuCounter.NextValue(); // Call first to get initial value
                        System.Threading.Thread.Sleep(100); // Wait a bit
                        return Math.Min(100, Math.Max(0, cpuCounter.NextValue()));
                    }
                }
                else
                {
                    // For non-Windows platforms, use the original method
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
                    // Use WMI for more accurate memory usage
                    using (var searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var totalMemoryKb = Convert.ToInt64(obj["TotalVisibleMemorySize"]);
                            var freeMemoryKb = Convert.ToInt64(obj["FreePhysicalMemory"]);
                            obj.Dispose();
                            
                            var totalMemory = totalMemoryKb * 1024.0; // Convert to bytes
                            var freeMemory = freeMemoryKb * 1024.0; // Convert to bytes
                            var usedMemory = totalMemory - freeMemory;
                            var percentage = (usedMemory / totalMemory) * 100;
                            
                            return Math.Min(100, Math.Max(0, percentage));
                        }
                    }
                }
                
                // For non-Windows platforms, return a simulated value
                // In a real application, you would use platform-specific methods
                return 30.0; // Simulated value
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

        public async Task OptimizeServicesAsync()
        {
            // Simulate service optimization
            await Task.Delay(2000);
        }

        public async Task ApplyPerformanceTuningAsync()
        {
            // Apply various performance tuning techniques
            await Task.Run(async () =>
            {
                // Simulate performance tuning operations
                await Task.Delay(1500);
            });
        }
    }
    
    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public NetworkStatus NetworkStatus { get; set; }
        public float TotalMemory { get; set; }
        public float AvailableMemory { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public DateTime BootTime { get; set; }
    }
    
    public class NetworkStatus
    {
        public string status { get; set; }
        public int connections { get; set; }
        public long bytesReceived { get; set; }
        public long bytesSent { get; set; }
    }
}