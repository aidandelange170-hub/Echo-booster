using System;
using System.Windows;
using System.Windows.Controls;

namespace EchoBooster
{
    public partial class PerformanceDetails : UserControl
    {
        private SystemBooster _booster;

        public PerformanceDetails(SystemBooster booster)
        {
            InitializeComponent();
            _booster = booster;
            UpdatePerformanceMetrics();
        }

        public void UpdatePerformanceMetrics()
        {
            var metrics = _booster.GetSystemMetrics();
            
            // Update performance metrics
            CpuUsageBar.Value = metrics.CpuUsage;
            MemoryUsageBar.Value = metrics.MemoryUsage;
            DiskUsageBar.Value = metrics.DiskUsage;
            NetworkUsageBar.Value = Math.Min(100, metrics.NetworkStatus.connections * 5); // Scale connections
            
            CpuUsageValue.Text = $"{metrics.CpuUsage:F1}%";
            MemoryUsageValue.Text = $"{metrics.MemoryUsage:F1}%";
            DiskUsageValue.Text = $"{metrics.DiskUsage:F1}%";
            NetworkUsageValue.Text = $"{metrics.NetworkStatus.connections} connections";
            
            // Update resource usage
            TotalMemoryText.Text = $"{metrics.TotalMemory:F0} MB";
            AvailableMemoryText.Text = $"{metrics.AvailableMemory:F0} MB";
            UsedMemoryText.Text = $"{(metrics.TotalMemory - metrics.AvailableMemory):F0} MB";
            
            ProcessCountText.Text = metrics.ProcessCount.ToString();
            ThreadCountText.Text = metrics.ThreadCount.ToString();
            HandleCountText.Text = metrics.HandleCount.ToString();
            
            // Calculate and display uptime
            var uptime = DateTime.Now - metrics.BootTime;
            UptimeText.Text = $"{uptime.Days} days, {uptime.Hours}h {uptime.Minutes}m";
            
            CpuCountText.Text = Environment.ProcessorCount.ToString();
            LogicalProcessorText.Text = Environment.ProcessorCount.ToString(); // On Windows, this is typically the same
            
            // Update system information
            OsText.Text = Environment.OSVersion.ToString();
            ArchText.Text = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            
            // These would be retrieved from system information in a real application
            ManufacturerText.Text = "N/A";
            ModelText.Text = "N/A";
            BiosText.Text = "N/A";
            BootTimeText.Text = metrics.BootTime.ToString("yyyy-MM-dd HH:mm:ss");
            
            // Generate recommendations based on metrics
            GenerateRecommendations(metrics);
        }

        private void GenerateRecommendations(SystemMetrics metrics)
        {
            var recommendations = new System.Text.StringBuilder();
            
            if (metrics.CpuUsage > 80)
            {
                recommendations.AppendLine("• CPU usage is high. Consider closing unnecessary applications.");
            }
            
            if (metrics.MemoryUsage > 85)
            {
                recommendations.AppendLine("• Memory usage is high. Consider closing memory-intensive applications.");
            }
            
            if (metrics.DiskUsage > 90)
            {
                recommendations.AppendLine("• Disk usage is high. Consider cleaning up disk space.");
            }
            
            if (metrics.ProcessCount > 100) // This is an arbitrary threshold
            {
                recommendations.AppendLine("• High number of running processes detected. Consider closing unused applications.");
            }
            
            if (recommendations.Length == 0)
            {
                RecommendationText.Text = "No recommendations at this time. System is running optimally.";
            }
            else
            {
                RecommendationText.Text = recommendations.ToString();
            }
        }

        private void OptimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            // This will be handled by the main window
            if (this.Parent is StackPanel panel && panel.Parent is ScrollViewer scroll && scroll.Parent is UserControl userControl)
            {
                Window parentWindow = Window.GetWindow(userControl);
                if (parentWindow is MainWindow mainWindow)
                {
                    mainWindow.IntelligentOptimizeFromDashboard();
                }
            }
        }
    }
}