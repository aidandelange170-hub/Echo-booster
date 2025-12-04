using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace EchoBooster
{
    public class SecurityMonitor
    {
        public class SecurityThreat
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Severity { get; set; }
            public string Description { get; set; }
            public DateTime DetectedTime { get; set; }
            public string Location { get; set; }
        }

        public class SecurityMetrics
        {
            public List<SecurityThreat> Threats { get; set; } = new List<SecurityThreat>();
            public int ActiveFirewallRules { get; set; }
            public bool AntivirusRunning { get; set; }
            public int SuspiciousProcesses { get; set; }
            public int NetworkConnections { get; set; }
            public int OpenPorts { get; set; }
            public bool SystemIntegrity { get; set; }
            public string SecurityScore { get; set; }
        }

        private List<string> _suspiciousProcessNames = new List<string>
        {
            "keylogger", "trojan", "virus", "malware", "rootkit", "spyware", 
            "adware", "worm", "backdoor", "botnet", "ransomware", "exploit"
        };

        private List<string> _suspiciousServices = new List<string>
        {
            "suspicious", "malicious", "unauthorized"
        };

        public SecurityMetrics GetSecurityMetrics()
        {
            var metrics = new SecurityMetrics
            {
                ActiveFirewallRules = GetActiveFirewallRules(),
                AntivirusRunning = IsAntivirusRunning(),
                SuspiciousProcesses = GetSuspiciousProcesses(),
                NetworkConnections = GetNetworkConnections(),
                OpenPorts = GetOpenPorts(),
                SystemIntegrity = CheckSystemIntegrity(),
                SecurityScore = CalculateSecurityScore()
            };

            metrics.Threats = DetectSecurityThreats();

            return metrics;
        }

        private List<SecurityThreat> DetectSecurityThreats()
        {
            var threats = new List<SecurityThreat>();

            // Detect suspicious processes
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    var processName = process.ProcessName.ToLower();
                    if (_suspiciousProcessNames.Any(susp => processName.Contains(susp)))
                    {
                        threats.Add(new SecurityThreat
                        {
                            Name = process.ProcessName,
                            Type = "Process",
                            Severity = "High",
                            Description = "Suspicious process detected",
                            DetectedTime = DateTime.Now,
                            Location = process.MainModule?.FileName ?? "Unknown"
                        });
                    }
                }
                catch
                {
                    // Ignore processes that can't be accessed
                }
            }

            // Detect suspicious services
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service"))
                {
                    foreach (ManagementObject service in searcher.Get())
                    {
                        var serviceName = service["Name"]?.ToString().ToLower();
                        if (!string.IsNullOrEmpty(serviceName) && 
                            _suspiciousServices.Any(susp => serviceName.Contains(susp)))
                        {
                            threats.Add(new SecurityThreat
                            {
                                Name = service["Name"]?.ToString(),
                                Type = "Service",
                                Severity = "Medium",
                                Description = "Suspicious service detected",
                                DetectedTime = DateTime.Now,
                                Location = service["PathName"]?.ToString() ?? "Unknown"
                            });
                        }
                        service.Dispose();
                    }
                }
            }
            catch
            {
                // Ignore if we can't access services
            }

            // Detect network threats
            var networkConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            foreach (var conn in networkConnections)
            {
                if (conn.RemoteEndPoint.Port < 1024 && conn.RemoteEndPoint.Address.ToString().StartsWith("192.168."))
                {
                    // Internal network connection to privileged port - potential threat
                    threats.Add(new SecurityThreat
                    {
                        Name = $"Network Connection to {conn.RemoteEndPoint}",
                        Type = "Network",
                        Severity = "Medium",
                        Description = "Unusual network connection to internal privileged port",
                        DetectedTime = DateTime.Now,
                        Location = "Network"
                    });
                }
            }

            return threats;
        }

        private int GetActiveFirewallRules()
        {
            try
            {
                // This is a simplified check - in a real application, you'd query the firewall directly
                // For now, we'll return a placeholder value
                return 42; // Simulated number of active firewall rules
            }
            catch
            {
                return 0;
            }
        }

        private bool IsAntivirusRunning()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM AntiVirusProduct", "root\\SecurityCenter2"))
                {
                    foreach (ManagementObject antivirus in searcher.Get())
                    {
                        var name = antivirus["displayName"]?.ToString();
                        var productState = antivirus["productState"]?.ToString();
                        antivirus.Dispose();
                        
                        // Product state: first 3 digits indicate firewall status, next 3 real-time protection, last 3 antispyware
                        if (!string.IsNullOrEmpty(productState) && productState.Length >= 6)
                        {
                            var realTimeProtection = productState.Substring(3, 3);
                            return realTimeProtection == "262144" || realTimeProtection == "266240"; // Enabled states
                        }
                    }
                }
            }
            catch
            {
                // Fallback: assume antivirus is running if we can't check
                return true;
            }

            return false;
        }

        private int GetSuspiciousProcesses()
        {
            var count = 0;
            var processes = Process.GetProcesses();
            
            foreach (var process in processes)
            {
                try
                {
                    var processName = process.ProcessName.ToLower();
                    if (_suspiciousProcessNames.Any(susp => processName.Contains(susp)))
                    {
                        count++;
                    }
                }
                catch
                {
                    // Ignore processes that can't be accessed
                }
            }
            
            return count;
        }

        private int GetNetworkConnections()
        {
            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnections = properties.GetActiveTcpConnections();
                return tcpConnections.Length;
            }
            catch
            {
                return 0;
            }
        }

        private int GetOpenPorts()
        {
            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpListeners = properties.GetActiveTcpListeners();
                var udpListeners = properties.GetActiveUdpListeners();
                return tcpListeners.Length + udpListeners.Length;
            }
            catch
            {
                return 0;
            }
        }

        private bool CheckSystemIntegrity()
        {
            // This is a simplified integrity check
            // In a real application, you'd check system file hashes, registry integrity, etc.
            try
            {
                // Check if critical system files exist and haven't been modified
                var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                var kernelFile = Path.Combine(systemDir, "ntoskrnl.exe");
                
                if (File.Exists(kernelFile))
                {
                    var fileInfo = new FileInfo(kernelFile);
                    // Check if file was modified recently (simplified check)
                    var timeSinceModified = DateTime.Now - fileInfo.LastWriteTime;
                    return timeSinceModified.TotalDays > 30; // If modified in last 30 days, flag as potentially compromised
                }
                
                return true; // Assume integrity if we can't check
            }
            catch
            {
                return true; // Assume integrity if we can't check
            }
        }

        private string CalculateSecurityScore()
        {
            var metrics = GetSecurityMetrics();
            var score = 100;

            // Deduct points for threats
            score -= metrics.Threats.Count * 10;
            
            // Deduct points if antivirus is not running
            if (!metrics.AntivirusRunning) score -= 20;
            
            // Deduct points for suspicious processes
            score -= metrics.SuspiciousProcesses * 5;
            
            // Add points for active firewall rules
            score += Math.Min(metrics.ActiveFirewallRules / 10, 10);
            
            // Ensure score is between 0 and 100
            score = Math.Max(0, Math.Min(100, score));
            
            if (score >= 80) return "Excellent";
            if (score >= 60) return "Good";
            if (score >= 40) return "Fair";
            return "Poor";
        }
    }
}