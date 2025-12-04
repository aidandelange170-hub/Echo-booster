using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EchoBooster
{
    public partial class DashboardView : UserControl
    {
        private SystemBooster _booster;
        private List<double> _cpuHistory = new List<double>();
        private List<double> _memoryHistory = new List<double>();

        public DashboardView(SystemBooster booster)
        {
            InitializeComponent();
            _booster = booster;
            UpdateSystemMetrics();
        }

        public void UpdateSystemMetrics()
        {
            // Get real metrics from the booster
            var metrics = _booster.GetSystemMetrics();
            
            // Update UI elements
            CpuUsageText.Text = $"{metrics.CpuUsage:F1}%";
            MemoryUsageText.Text = $"{metrics.MemoryUsage:F1}%";
            DiskUsageText.Text = $"{metrics.DiskUsage:F1}%";
            NetworkStatusText.Text = metrics.NetworkStatus.status;
            
            CpuProgressBar.Value = metrics.CpuUsage;
            MemoryProgressBar.Value = metrics.MemoryUsage;
            DiskProgressBar.Value = metrics.DiskUsage;
            NetworkProgressBar.Value = Math.Min(100, metrics.NetworkStatus.connections * 5); // Scale connections to progress bar
            
            // Update system information
            TotalMemoryText.Text = $"{metrics.TotalMemory:F0} MB";
            AvailableMemoryText.Text = $"{metrics.AvailableMemory:F0} MB";
            ProcessCountText.Text = metrics.ProcessCount.ToString();
            ThreadCountText.Text = metrics.ThreadCount.ToString();
            
            // Calculate and display uptime
            var uptime = DateTime.Now - metrics.BootTime;
            UptimeText.Text = $"{uptime.Days} days, {uptime.Hours}h {uptime.Minutes}m";
            BootTimeText.Text = metrics.BootTime.ToString("yyyy-MM-dd HH:mm:ss");
            
            // Update history for charts
            _cpuHistory.Add(metrics.CpuUsage);
            _memoryHistory.Add(metrics.MemoryUsage);
            
            // Keep only last 50 values for performance
            if (_cpuHistory.Count > 50) _cpuHistory.RemoveAt(0);
            if (_memoryHistory.Count > 50) _memoryHistory.RemoveAt(0);
            
            // Draw charts
            DrawCpuChart();
            DrawMemoryChart();
        }

        private void DrawCpuChart()
        {
            CpuChartCanvas.Children.Clear();
            if (_cpuHistory.Count < 2) return;

            double width = CpuChartCanvas.ActualWidth;
            double height = CpuChartCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            double stepX = width / (_cpuHistory.Count - 1);
            double maxValue = 100;

            PointCollection points = new PointCollection();
            points.Add(new Point(0, height)); // Start at bottom left

            for (int i = 0; i < _cpuHistory.Count; i++)
            {
                double x = i * stepX;
                double y = height - (_cpuHistory[i] / maxValue) * height;
                points.Add(new Point(x, y));
            }

            points.Add(new Point(width, height)); // End at bottom right

            Polygon polygon = new Polygon
            {
                Points = points,
                Fill = new SolidColorBrush(Color.FromArgb(100, 52, 152, 219)), // Semi-transparent blue
                Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185)), // Solid blue border
                StrokeThickness = 2
            };

            CpuChartCanvas.Children.Add(polygon);
        }

        private void DrawMemoryChart()
        {
            MemoryChartCanvas.Children.Clear();
            if (_memoryHistory.Count < 2) return;

            double width = MemoryChartCanvas.ActualWidth;
            double height = MemoryChartCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            double stepX = width / (_memoryHistory.Count - 1);
            double maxValue = 100;

            PointCollection points = new PointCollection();
            points.Add(new Point(0, height)); // Start at bottom left

            for (int i = 0; i < _memoryHistory.Count; i++)
            {
                double x = i * stepX;
                double y = height - (_memoryHistory[i] / maxValue) * height;
                points.Add(new Point(x, y));
            }

            points.Add(new Point(width, height)); // End at bottom right

            Polygon polygon = new Polygon
            {
                Points = points,
                Fill = new SolidColorBrush(Color.FromArgb(100, 231, 76, 60)), // Semi-transparent red
                Stroke = new SolidColorBrush(Color.FromArgb(255, 192, 57, 43)), // Solid red border
                StrokeThickness = 2
            };

            MemoryChartCanvas.Children.Add(polygon);
        }

        private void IntelligentOptimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            // This will be handled by the main window
            if (this.Parent is StackPanel panel && panel.Parent is ScrollViewer scroll && scroll.Parent is UserControl userControl)
            {
                // Find the parent window to handle the event
                Window parentWindow = Window.GetWindow(userControl);
                if (parentWindow is MainWindow mainWindow)
                {
                    mainWindow.IntelligentOptimizeFromDashboard();
                }
            }
        }

        private void OptimizeSystemBtn_Click(object sender, RoutedEventArgs e)
        {
            // This will be handled by the main window
            if (this.Parent is StackPanel panel && panel.Parent is ScrollViewer scroll && scroll.Parent is UserControl userControl)
            {
                Window parentWindow = Window.GetWindow(userControl);
                if (parentWindow is MainWindow mainWindow)
                {
                    mainWindow.OptimizeSystemFromDashboard();
                }
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateSystemMetrics();
        }

        private void StopMonitoringBtn_Click(object sender, RoutedEventArgs e)
        {
            _booster.StopMonitoring();
        }
    }
}