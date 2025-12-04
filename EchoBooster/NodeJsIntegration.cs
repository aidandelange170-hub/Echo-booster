using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace EchoBooster
{
    public class NodeJsIntegration
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:3000";

        public NodeJsIntegration()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // Set a reasonable timeout
        }

        public async Task<SystemMetrics> GetSystemMetricsFromNodeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/metrics");
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var nodeMetrics = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                    // Map Node.js metrics to our SystemMetrics model
                    var metrics = new SystemMetrics
                    {
                        CpuUsage = nodeMetrics.GetProperty("cpu").GetDouble(),
                        MemoryUsage = nodeMetrics.GetProperty("memory").GetDouble(),
                        DiskUsage = nodeMetrics.GetProperty("disk").GetDouble(),
                        ProcessCount = nodeMetrics.GetProperty("processes").GetProperty("list").GetArrayLength(),
                        // Additional metrics
                        TotalMemory = 8192, // Default value, Node.js systeminformation can provide this
                        AvailableMemory = 4096, // Default value
                        BootTime = DateTime.Now.AddHours(-1) // Default value
                    };

                    // Create a basic NetworkStatus
                    metrics.NetworkStatus = new NetworkStatus
                    {
                        status = nodeMetrics.GetProperty("network").GetInt32() > 0 ? "Connected" : "Disconnected",
                        connections = nodeMetrics.GetProperty("network").GetInt32(),
                        bytesReceived = 0,
                        bytesSent = 0
                    };

                    return metrics;
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"Error getting metrics from Node.js server: {ex.Message}");
            }

            // Return default metrics if the Node.js server is not available
            return new SystemMetrics
            {
                CpuUsage = 0,
                MemoryUsage = 0,
                DiskUsage = 0,
                NetworkStatus = new NetworkStatus { status = "Unavailable", connections = 0, bytesReceived = 0, bytesSent = 0 },
                TotalMemory = 0,
                AvailableMemory = 0,
                ProcessCount = 0,
                ThreadCount = 0,
                HandleCount = 0,
                BootTime = DateTime.MinValue
            };
        }

        public async Task<bool> OptimizeSystemViaNodeAsync(string optimizationType = "all")
        {
            try
            {
                var payload = new { type = optimizationType };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/optimize", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending optimization request to Node.js server: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StartNodeServerAsync()
        {
            // In a real implementation, this would start the Node.js server
            // For now, we'll just check if it's running
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/metrics");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}