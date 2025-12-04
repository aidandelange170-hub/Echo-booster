using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace EchoBooster
{
    public class SystemHealthChecker
    {
        public class HealthCheckResult
        {
            public bool IsHealthy { get; set; }
            public string OverallStatus { get; set; }
            public int HealthScore { get; set; }
            public List<HealthIssue> Issues { get; set; } = new List<HealthIssue>();
            public DateTime CheckedAt { get; set; }
        }

        public class HealthIssue
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public string Severity { get; set; } // "Critical", "High", "Medium", "Low"
            public string Description { get; set; }
            public string Recommendation { get; set; }
            public DateTime DetectedAt { get; set; }
        }

        private SystemBooster _booster;
        private SecurityMonitor _securityMonitor;
        private SystemCleanup _cleanup;

        public SystemHealthChecker(SystemBooster booster, SecurityMonitor securityMonitor, SystemCleanup cleanup)
        {
            _booster = booster;
            _securityMonitor = securityMonitor;
            _cleanup = cleanup;
        }

        public async Task<HealthCheckResult> PerformHealthCheckAsync()
        {
            var result = new HealthCheckResult();
            result.CheckedAt = DateTime.Now;
            var issues = new List<HealthIssue>();

            // Check system performance
            var performanceIssues = await CheckPerformanceHealthAsync();
            issues.AddRange(performanceIssues);

            // Check security
            var securityIssues = CheckSecurityHealth();
            issues.AddRange(securityIssues);

            // Check disk space
            var diskIssues = CheckDiskHealth();
            issues.AddRange(diskIssues);

            // Check system stability
            var stabilityIssues = CheckSystemStability();
            issues.AddRange(stabilityIssues);

            // Calculate overall health
            result.Issues = issues;
            result.HealthScore = CalculateHealthScore(issues);
            result.OverallStatus = DetermineOverallStatus(result.HealthScore);
            result.IsHealthy = result.HealthScore >= 70;

            return result;
        }

        private async Task<List<HealthIssue>> CheckPerformanceHealthAsync()
        {
            var issues = new List<HealthIssue>();
            var metrics = _booster.GetSystemMetrics();

            // Check CPU usage
            if (metrics.CpuUsage > 90)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Performance",
                    Name = "High CPU Usage",
                    Severity = "High",
                    Description = $"CPU usage is at {metrics.CpuUsage:F1}% which is critically high",
                    Recommendation = "Close unnecessary applications or check for CPU-intensive processes",
                    DetectedAt = DateTime.Now
                });
            }
            else if (metrics.CpuUsage > 75)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Performance",
                    Name = "Elevated CPU Usage",
                    Severity = "Medium",
                    Description = $"CPU usage is at {metrics.CpuUsage:F1}% which is elevated",
                    Recommendation = "Monitor system performance for potential bottlenecks",
                    DetectedAt = DateTime.Now
                });
            }

            // Check memory usage
            if (metrics.MemoryUsage > 90)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Performance",
                    Name = "High Memory Usage",
                    Severity = "High",
                    Description = $"Memory usage is at {metrics.MemoryUsage:F1}% which is critically high",
                    Recommendation = "Close unnecessary applications or consider adding more RAM",
                    DetectedAt = DateTime.Now
                });
            }
            else if (metrics.MemoryUsage > 80)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Performance",
                    Name = "Elevated Memory Usage",
                    Severity = "Medium",
                    Description = $"Memory usage is at {metrics.MemoryUsage:F1}% which is elevated",
                    Recommendation = "Monitor memory usage for potential leaks",
                    DetectedAt = DateTime.Now
                });
            }

            // Check disk usage
            if (metrics.DiskUsage > 95)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Performance",
                    Name = "Critical Disk Usage",
                    Severity = "High",
                    Description = $"Disk usage is at {metrics.DiskUsage:F1}% which is critically high",
                    Recommendation = "Clean up disk space immediately",
                    DetectedAt = DateTime.Now
                });
            }
            else if (metrics.DiskUsage > 90)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Performance",
                    Name = "High Disk Usage",
                    Severity = "Medium",
                    Description = $"Disk usage is at {metrics.DiskUsage:F1}% which is high",
                    Recommendation = "Consider cleaning up disk space",
                    DetectedAt = DateTime.Now
                });
            }

            // Check process count
            if (metrics.ProcessCount > 200)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Performance",
                    Name = "High Process Count",
                    Severity = "Medium",
                    Description = $"Running {metrics.ProcessCount} processes which is high",
                    Recommendation = "Check for unnecessary background processes",
                    DetectedAt = DateTime.Now
                });
            }

            return issues;
        }

        private List<HealthIssue> CheckSecurityHealth()
        {
            var issues = new List<HealthIssue>();
            var securityMetrics = _securityMonitor.GetSecurityMetrics();

            // Check if antivirus is running
            if (!securityMetrics.AntivirusRunning)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Security",
                    Name = "Antivirus Not Running",
                    Severity = "High",
                    Description = "No active antivirus software detected",
                    Recommendation = "Install and enable antivirus software",
                    DetectedAt = DateTime.Now
                });
            }

            // Check for security threats
            foreach (var threat in securityMetrics.Threats)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Security",
                    Name = threat.Name,
                    Severity = threat.Severity,
                    Description = threat.Description,
                    Recommendation = "Investigate and remove potential security threats",
                    DetectedAt = threat.DetectedTime
                });
            }

            // Check suspicious processes
            if (securityMetrics.SuspiciousProcesses > 0)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Security",
                    Name = "Suspicious Processes",
                    Severity = "High",
                    Description = $"Detected {securityMetrics.SuspiciousProcesses} potentially malicious processes",
                    Recommendation = "Scan system for malware and investigate suspicious processes",
                    DetectedAt = DateTime.Now
                });
            }

            // Check system integrity
            if (!securityMetrics.SystemIntegrity)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Security",
                    Name = "System Integrity Compromised",
                    Severity = "High",
                    Description = "System files may have been modified",
                    Recommendation = "Run system file checker and security scan",
                    DetectedAt = DateTime.Now
                });
            }

            return issues;
        }

        private List<HealthIssue> CheckDiskHealth()
        {
            var issues = new List<HealthIssue>();

            try
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
                {
                    var freeSpacePercentage = (drive.AvailableFreeSpace / (double)drive.TotalSize) * 100;
                    var usedSpacePercentage = 100 - freeSpacePercentage;

                    if (usedSpacePercentage > 95)
                    {
                        issues.Add(new HealthIssue
                        {
                            Category = "Disk",
                            Name = $"Critical Space on {drive.Name}",
                            Severity = "High",
                            Description = $"Drive {drive.Name} is {usedSpacePercentage:F1}% full with only {FormatFileSize(drive.AvailableFreeSpace)} free",
                            Recommendation = "Free up disk space immediately",
                            DetectedAt = DateTime.Now
                        });
                    }
                    else if (usedSpacePercentage > 90)
                    {
                        issues.Add(new HealthIssue
                        {
                            Category = "Disk",
                            Name = $"Low Space on {drive.Name}",
                            Severity = "Medium",
                            Description = $"Drive {drive.Name} is {usedSpacePercentage:F1}% full with only {FormatFileSize(drive.AvailableFreeSpace)} free",
                            Recommendation = "Consider cleaning up disk space",
                            DetectedAt = DateTime.Now
                        });
                    }

                    // Check for bad sectors (simplified check)
                    try
                    {
                        // This is a simplified check - in reality, you'd use WMI or other methods to check for bad sectors
                        var testFile = Path.Combine(drive.RootDirectory.FullName, "health_test.tmp");
                        File.WriteAllText(testFile, "test");
                        File.Delete(testFile);
                    }
                    catch
                    {
                        issues.Add(new HealthIssue
                        {
                            Category = "Disk",
                            Name = $"Potential Disk Issues on {drive.Name}",
                            Severity = "Medium",
                            Description = $"Drive {drive.Name} may have read/write issues",
                            Recommendation = "Run disk check utility to scan for errors",
                            DetectedAt = DateTime.Now
                        });
                    }
                }
            }
            catch
            {
                issues.Add(new HealthIssue
                {
                    Category = "Disk",
                    Name = "Disk Health Check Failed",
                    Severity = "Medium",
                    Description = "Unable to check disk health",
                    Recommendation = "Check disk access permissions and try again",
                    DetectedAt = DateTime.Now
                });
            }

            return issues;
        }

        private List<HealthIssue> CheckSystemStability()
        {
            var issues = new List<HealthIssue>();

            try
            {
                // Check system uptime
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                if (uptime.TotalDays > 30)
                {
                    issues.Add(new HealthIssue
                    {
                        Category = "Stability",
                        Name = "Long System Uptime",
                        Severity = "Low",
                        Description = $"System has been running for {uptime.Days} days without restart",
                        Recommendation = "Consider restarting to clear memory and refresh system services",
                        DetectedAt = DateTime.Now
                    });
                }

                // Check for recent system crashes (simplified check)
                var windowsLogs = CheckEventLogsForCrashes();
                if (windowsLogs > 0)
                {
                    issues.Add(new HealthIssue
                    {
                        Category = "Stability",
                        Name = "Recent System Crashes",
                        Severity = "High",
                        Description = $"Detected {windowsLogs} potential system crashes in recent logs",
                        Recommendation = "Check system event logs for details and update drivers if needed",
                        DetectedAt = DateTime.Now
                    });
                }

                // Check for high thread count
                var metrics = _booster.GetSystemMetrics();
                if (metrics.ThreadCount > 5000)
                {
                    issues.Add(new HealthIssue
                    {
                        Category = "Stability",
                        Name = "High Thread Count",
                        Severity = "Medium",
                        Description = $"System has {metrics.ThreadCount} threads running which may affect stability",
                        Recommendation = "Monitor for applications creating excessive threads",
                        DetectedAt = DateTime.Now
                    });
                }
            }
            catch
            {
                issues.Add(new HealthIssue
                {
                    Category = "Stability",
                    Name = "Stability Check Failed",
                    Severity = "Low",
                    Description = "Unable to check system stability",
                    Recommendation = "Check system permissions and try again",
                    DetectedAt = DateTime.Now
                });
            }

            return issues;
        }

        private int CheckEventLogsForCrashes()
        {
            try
            {
                using (var eventLog = new EventLog("System"))
                {
                    var entries = eventLog.Entries.Cast<EventLogEntry>()
                        .Where(e => e.TimeGenerated >= DateTime.Now.AddDays(-7) && 
                                   (e.EntryType == EventLogEntryType.Error || e.EntryType == EventLogEntryType.Warning) &&
                                   (e.Message.Contains("crash", StringComparison.OrdinalIgnoreCase) ||
                                    e.Message.Contains("blue screen", StringComparison.OrdinalIgnoreCase) ||
                                    e.Message.Contains("critical", StringComparison.OrdinalIgnoreCase) ||
                                    e.Message.Contains("kernel", StringComparison.OrdinalIgnoreCase) ||
                                    e.Message.Contains("driver", StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    return entries.Count;
                }
            }
            catch
            {
                return 0; // If we can't check event logs, assume no crashes
            }
        }

        private int CalculateHealthScore(List<HealthIssue> issues)
        {
            int score = 100;

            foreach (var issue in issues)
            {
                switch (issue.Severity.ToLower())
                {
                    case "critical":
                        score -= 25;
                        break;
                    case "high":
                        score -= 15;
                        break;
                    case "medium":
                        score -= 8;
                        break;
                    case "low":
                        score -= 3;
                        break;
                }
            }

            // Ensure score is between 0 and 100
            return Math.Max(0, Math.Min(100, score));
        }

        private string DetermineOverallStatus(int healthScore)
        {
            if (healthScore >= 90) return "Excellent";
            if (healthScore >= 75) return "Good";
            if (healthScore >= 60) return "Fair";
            if (healthScore >= 40) return "Poor";
            return "Critical";
        }

        public async Task<List<HealthIssue>> GetCriticalIssuesAsync()
        {
            var result = await PerformHealthCheckAsync();
            return result.Issues.Where(i => 
                i.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase) || 
                i.Severity.Equals("High", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<bool> HasCriticalIssuesAsync()
        {
            var criticalIssues = await GetCriticalIssuesAsync();
            return criticalIssues.Count > 0;
        }

        public async Task<string> GenerateHealthReportAsync()
        {
            var result = await PerformHealthCheckAsync();
            var report = new System.Text.StringBuilder();

            report.AppendLine("=== SYSTEM HEALTH REPORT ===");
            report.AppendLine($"Generated: {result.CheckedAt:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Overall Status: {result.OverallStatus} (Score: {result.HealthScore}/100)");
            report.AppendLine($"System Health: {(result.IsHealthy ? "Healthy" : "Unhealthy")}");
            report.AppendLine();

            if (result.Issues.Any())
            {
                report.AppendLine("DETECTED ISSUES:");
                foreach (var issue in result.Issues)
                {
                    report.AppendLine($"  [{issue.Severity}] {issue.Category}: {issue.Name}");
                    report.AppendLine($"      {issue.Description}");
                    report.AppendLine($"      Recommendation: {issue.Recommendation}");
                    report.AppendLine();
                }
            }
            else
            {
                report.AppendLine("No issues detected. System is in good health!");
            }

            return report.ToString();
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
    }
}