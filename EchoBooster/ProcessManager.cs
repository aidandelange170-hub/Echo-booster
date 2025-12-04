using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace EchoBooster
{
    public class ProcessManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public class ProcessInfo
        {
            public int ProcessId { get; set; }
            public string ProcessName { get; set; }
            public double CpuUsage { get; set; }
            public long MemoryUsage { get; set; } // in MB
            public int ThreadCount { get; set; }
            public TimeSpan Uptime { get; set; }
            public bool IsCritical { get; set; }
        }

        private Dictionary<int, PerformanceCounter> _cpuCounters = new Dictionary<int, PerformanceCounter>();
        private PerformanceCounter _totalCpuCounter;

        public ProcessManager()
        {
            _totalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _totalCpuCounter.NextValue(); // Call first to get initial value
        }

        public List<ProcessInfo> GetProcessesWithUsage()
        {
            var processes = new List<ProcessInfo>();
            
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    var procInfo = new ProcessInfo
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        ThreadCount = process.Threads.Count,
                        MemoryUsage = process.WorkingSet64 / (1024 * 1024), // Convert to MB
                        IsCritical = IsSystemCriticalProcess(process.ProcessName),
                        Uptime = DateTime.Now - process.StartTime
                    };

                    // Calculate CPU usage for this process
                    procInfo.CpuUsage = GetProcessCpuUsage(process);
                    
                    processes.Add(procInfo);
                }
                catch
                {
                    // Skip processes that can't be accessed
                }
            }

            return processes;
        }

        private double GetProcessCpuUsage(Process process)
        {
            try
            {
                if (!_cpuCounters.ContainsKey(process.Id))
                {
                    _cpuCounters[process.Id] = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                    _cpuCounters[process.Id].NextValue(); // Prime the counter
                    Task.Delay(100).Wait(); // Wait a bit before getting the next value
                }

                return Math.Round(_cpuCounters[process.Id].NextValue() / Environment.ProcessorCount, 2);
            }
            catch
            {
                // If we can't get the actual CPU usage, return 0
                return 0;
            }
        }
        
        public double GetSystemCpuUsage()
        {
            try
            {
                if (_totalCpuCounter != null)
                {
                    // Get the current value
                    float value = _totalCpuCounter.NextValue();
                    Task.Delay(100).Wait(); // Wait before the next read
                    return Math.Round(_totalCpuCounter.NextValue(), 2);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public string GetActiveApplicationName()
        {
            try
            {
                IntPtr handle = GetForegroundWindow();
                GetWindowThreadProcessId(handle, out uint processId);
                
                if (processId != 0)
                {
                    var process = Process.GetProcessById((int)processId);
                    return process.ProcessName;
                }
            }
            catch
            {
                // If we can't get the active application, return a default value
            }
            
            return "Unknown";
        }

        public async Task CloseBackgroundProcesses()
        {
            try
            {
                string activeAppName = GetActiveApplicationName();
                
                var processes = Process.GetProcesses();
                var processesToClose = new List<Process>();
                
                foreach (var process in processes)
                {
                    try
                    {
                        // Skip system critical processes and the currently active application
                        if (!IsSystemCriticalProcess(process.ProcessName) && 
                            !process.ProcessName.Equals(activeAppName, StringComparison.OrdinalIgnoreCase) &&
                            !IsCurrentApplication(process.ProcessName))
                        {
                            processesToClose.Add(process);
                        }
                    }
                    catch
                    {
                        // Skip processes that can't be accessed
                    }
                }

                // Close the background processes
                foreach (var process in processesToClose)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(2000); // Wait up to 2 seconds for process to exit
                    }
                    catch
                    {
                        // If the process couldn't be killed, continue with others
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error closing background processes: {ex.Message}");
            }
        }

        private bool IsSystemCriticalProcess(string processName)
        {
            var criticalProcesses = new[]
            {
                "System", "svchost", "explorer", "winlogon", "csrss",
                "lsass", "services", "wininit", "smss", "System Idle Process",
                "winmgmt", "spoolsv", "taskhost", "dwm", "ctfmon",
                "ntoskrnl", "csrss", "lsm", "TrustedInstaller", "MsMpEng",
                "SecurityHealthService", "audiodg", "SearchIndexer"
            };

            return criticalProcesses.Contains(processName, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsCurrentApplication(string processName)
        {
            // Get the name of the current application (the EchoBooster app)
            string currentAppName = Process.GetCurrentProcess().ProcessName;
            return processName.Equals(currentAppName, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            foreach (var counter in _cpuCounters.Values)
            {
                counter?.Dispose();
            }
            _totalCpuCounter?.Dispose();
        }
    }
}