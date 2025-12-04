using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EchoBooster
{
    public partial class MainWindow : Window
    {
        private SystemBooster _booster;
        private SecurityMonitor _securityMonitor;
        private SystemCleanup _cleanup;
        private PerformanceScheduler _scheduler;
        private SystemHealthChecker _healthChecker;
        private bool _isMonitoring = false;
        private DispatcherTimer _updateTimer;
        private List<double> _cpuHistory = new List<double>();
        private List<double> _memoryHistory = new List<double>();
        private Random _random = new Random();
        private UserControl _currentView;
        private UpdateManager _updateManager;

        public MainWindow()
        {
            InitializeComponent();
            _booster = new SystemBooster();
            _securityMonitor = new SecurityMonitor();
            _cleanup = new SystemCleanup();
            _updateManager = new UpdateManager(this);
            _scheduler = new PerformanceScheduler(_booster, _cleanup, _updateManager);
            _healthChecker = new SystemHealthChecker(_booster, _securityMonitor, _cleanup);
            _booster.StartMonitoring();
            InitializeTimer();
            UpdateSystemMetrics();
            
            // Initialize version display
            VersionText.Text = "v3.1";
            SystemInfoText.Text = GetOperatingSystemInfo();
            
            // Check for cached updates from previous session first
            Task.Run(async () =>
            {
                await _updateManager.CheckForCachedUpdatesAsync();
            });
            
            // Start background update checking (check every 30 minutes)
            Task.Run(async () =>
            {
                await _updateManager.StartBackgroundUpdateCheckingAsync(TimeSpan.FromMinutes(30));
            });
            
            // Start auto-update check after a delay
            Task.Delay(3000).ContinueWith(_ => 
            {
                _updateManager.CheckForUpdatesWithNotification();
            });
            
            // Schedule daily health checks
            Task.Run(async () =>
            {
                await _scheduler.ScheduleDailyOptimizationAsync(2, 0); // Run optimization at 2:00 AM
                await _scheduler.ScheduleWeeklyCleanupAsync(DayOfWeek.Sunday, 3, 0); // Run cleanup every Sunday at 3:00 AM
            });
        }

        private void InitializeTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateSystemMetrics();
        }

        private void UpdateSystemMetrics()
        {
            // Get real metrics from the booster
            var metrics = _booster.GetSystemMetrics();
            
            // Update UI elements
            CpuUsageText.Text = $"{metrics.CpuUsage:F1}%";
            MemoryUsageText.Text = $"{metrics.MemoryUsage:F1}%";
            DiskUsageText.Text = $"{metrics.DiskUsage:F1}%";
            
            CpuProgressBar.Value = metrics.CpuUsage;
            MemoryProgressBar.Value = metrics.MemoryUsage;
            DiskProgressBar.Value = metrics.DiskUsage;
            
            // Update history for charts
            _cpuHistory.Add(metrics.CpuUsage);
            _memoryHistory.Add(metrics.MemoryUsage);
            
            // Keep only last 50 values for performance
            if (_cpuHistory.Count > 50) _cpuHistory.RemoveAt(0);
            if (_memoryHistory.Count > 50) _memoryHistory.RemoveAt(0);
            
            // Draw charts
            DrawCpuChart();
            DrawMemoryChart();
            
            // Update status
            LastUpdatedText.Text = $"Last Updated: {DateTime.Now:HH:mm:ss}";
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

        private void SecurityBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Security";
            ContentPanel.Children.Clear();
            LoadSecurityContent();
        }

        private void HealthBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Health";
            ContentPanel.Children.Clear();
            LoadHealthContent();
        }

        private void DashboardBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Dashboard";
            ContentPanel.Children.Clear();
            LoadDashboardContent();
        }

        private void PerformanceBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Performance";
            ContentPanel.Children.Clear();
            LoadPerformanceContent();
        }

        private void ProcessesBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Processes";
            ContentPanel.Children.Clear();
            LoadProcessesContent();
        }

        private void NetworkBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Network";
            ContentPanel.Children.Clear();
            LoadNetworkContent();
        }

        private void OptimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Optimize";
            ContentPanel.Children.Clear();
            LoadOptimizeContent();
        }

        private void ResourceManagerBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Resource Manager";
            ContentPanel.Children.Clear();
            LoadResourceManagerContent();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            TitleText.Text = "Settings";
            ContentPanel.Children.Clear();
            LoadSettingsContent();
        }

        private void LoadDashboardContent()
        {
            var dashboardView = new DashboardView(_booster);
            ContentPanel.Children.Add(dashboardView);
        }

        private Border CreateMetricCard(string color, string title, string value, ProgressBar progressBar)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(15)
            };

            var stackPanel = new StackPanel();

            var titleLabel = new TextBlock
            {
                Text = title,
                FontSize = 14,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var valueLabel = new TextBlock
            {
                Text = value,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var progress = new ProgressBar
            {
                Height = 8,
                Margin = new Thickness(0, 10, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetDarkerColor(color))),
                Foreground = new SolidColorBrush(Color.FromRgb(26, 188, 156))
            };
            progress.Value = progressBar.Value;

            stackPanel.Children.Add(titleLabel);
            stackPanel.Children.Add(valueLabel);
            stackPanel.Children.Add(progress);

            border.Child = stackPanel;
            return border;
        }

        private string GetDarkerColor(string color)
        {
            // Convert hex color to darker version
            if (color == "#3498db") return "#2980b9";
            if (color == "#e74c3c") return "#c0392b";
            if (color == "#9b59b6") return "#8e44ad";
            if (color == "#f39c12") return "#d35400";
            return color;
        }

        private void LoadProcessesContent()
        {
            var processesView = new ProcessesView(_booster);
            ContentPanel.Children.Add(processesView);
        }

        private void LoadNetworkContent()
        {
            var networkView = new NetworkView(_booster);
            ContentPanel.Children.Add(networkView);
        }

        private void LoadSecurityContent()
        {
            var securityView = new SecurityView(_securityMonitor, _booster);
            ContentPanel.Children.Add(securityView);
        }

        private void LoadHealthContent()
        {
            var healthView = new HealthView(_healthChecker, _booster);
            ContentPanel.Children.Add(healthView);
        }

        private void LoadPerformanceContent()
        {
            var title = new TextBlock { Text = "Performance Details", FontSize = 24, FontWeight = "Bold", Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) };
            title.Margin = new Thickness(0, 0, 0, 20);
            ContentPanel.Children.Add(title);
            
            var subtitle = new TextBlock { Text = "Detailed system performance metrics", FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)) };
            subtitle.Margin = new Thickness(0, 0, 0, 30);
            ContentPanel.Children.Add(subtitle);
            
            var metrics = _booster.GetSystemMetrics();
            
            var grid = new Grid();
            grid.Margin = new Thickness(0, 0, 0, 20);
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            
            AddMetricRow(grid, 0, "Total Memory:", $"{metrics.TotalMemory:F0} MB");
            AddMetricRow(grid, 1, "Available Memory:", $"{metrics.AvailableMemory:F0} MB");
            AddMetricRow(grid, 2, "Used Memory:", $"{(metrics.TotalMemory - metrics.AvailableMemory):F0} MB");
            AddMetricRow(grid, 3, "Running Processes:", metrics.ProcessCount.ToString());
            AddMetricRow(grid, 4, "Total Threads:", metrics.ThreadCount.ToString());
            
            ContentPanel.Children.Add(grid);
        }

        private void LoadOptimizeContent()
        {
            var title = new TextBlock { Text = "System Optimization", FontSize = 24, FontWeight = "Bold", Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) };
            title.Margin = new Thickness(0, 0, 0, 20);
            ContentPanel.Children.Add(title);
            
            var subtitle = new TextBlock { Text = "Optimize your system performance", FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)) };
            subtitle.Margin = new Thickness(0, 0, 0, 30);
            ContentPanel.Children.Add(subtitle);
            
            var stackPanel = new StackPanel();
            
            var optimizeBtn = new Button { Content = "Optimize All Processes", Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)), Foreground = new SolidColorBrush(Colors.White), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 10) };
            optimizeBtn.Click += OptimizeSystemBtn_Click;
            stackPanel.Children.Add(optimizeBtn);
            
            var intelligentOptimizeBtn = new Button { Content = "Intelligent Optimization", Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)), Foreground = new SolidColorBrush(Colors.White), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 10) };
            intelligentOptimizeBtn.Click += IntelligentOptimizeBtn_Click;
            stackPanel.Children.Add(intelligentOptimizeBtn);
            
            var cleanMemoryBtn = new Button { Content = "Clean System Memory", Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)), Foreground = new SolidColorBrush(Colors.White), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 10) };
            cleanMemoryBtn.Click += CleanMemoryBtn_Click;
            stackPanel.Children.Add(cleanMemoryBtn);
            
            var reduceWorkingSetBtn = new Button { Content = "Reduce Working Set", Background = new SolidColorBrush(Color.FromRgb(241, 196, 15)), Foreground = new SolidColorBrush(Colors.White), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 10) };
            reduceWorkingSetBtn.Click += ReduceWorkingSetBtn_Click;
            stackPanel.Children.Add(reduceWorkingSetBtn);
            
            // New cleanup button
            var cleanupBtn = new Button { Content = "System Cleanup", Background = new SolidColorBrush(Color.FromRgb(230, 126, 34)), Foreground = new SolidColorBrush(Colors.White), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 10) };
            cleanupBtn.Click += CleanupBtn_Click;
            stackPanel.Children.Add(cleanupBtn);
            
            // New security scan button
            var securityScanBtn = new Button { Content = "Security Scan", Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)), Foreground = new SolidColorBrush(Colors.White), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 10) };
            securityScanBtn.Click += SecurityScanBtn_Click;
            stackPanel.Children.Add(securityScanBtn);
            
            // New health check button
            var healthCheckBtn = new Button { Content = "System Health Check", Background = new SolidColorBrush(Color.FromRgb(46, 134, 193)), Foreground = new SolidColorBrush(Colors.White), Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 0, 10) };
            healthCheckBtn.Click += HealthCheckBtn_Click;
            stackPanel.Children.Add(healthCheckBtn);
            
            ContentPanel.Children.Add(stackPanel);
        }

        private void LoadResourceManagerContent()
        {
            var title = new TextBlock { Text = "Resource Manager", FontSize = 24, FontWeight = "Bold", Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) };
            title.Margin = new Thickness(0, 0, 0, 20);
            ContentPanel.Children.Add(title);
            
            var subtitle = new TextBlock { Text = "Manage CPU usage and application resources", FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)) };
            subtitle.Margin = new Thickness(0, 0, 0, 30);
            ContentPanel.Children.Add(subtitle);
            
            // CPU Usage Section
            var cpuUsageGroup = new GroupBox { Header = "CPU Usage", Margin = new Thickness(0, 0, 0, 20) };
            var cpuUsageStack = new StackPanel();
            
            var cpuUsageLabel = new TextBlock { Text = $"Current CPU Usage: {CpuUsageText.Text}", FontSize = 14, Margin = new Thickness(0, 0, 0, 10) };
            cpuUsageStack.Children.Add(cpuUsageLabel);
            
            var cpuProgressBar = new ProgressBar { Height = 20, Margin = new Thickness(0, 0, 0, 10) };
            cpuProgressBar.Value = CpuProgressBar.Value;
            cpuUsageStack.Children.Add(cpuProgressBar);
            
            cpuUsageGroup.Content = cpuUsageStack;
            ContentPanel.Children.Add(cpuUsageGroup);
            
            // Resource Management Actions
            var actionsGroup = new GroupBox { Header = "Resource Management Actions", Margin = new Thickness(0, 0, 0, 20) };
            var actionsStack = new StackPanel { Orientation = Orientation.Vertical };
            
            var closeBackgroundAppsBtn = new Button 
            { 
                Content = "Close All Background Apps", 
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)), 
                Foreground = new SolidColorBrush(Colors.White), 
                Padding = new Thickness(15, 10, 15, 10), 
                Margin = new Thickness(0, 0, 0, 10) 
            };
            closeBackgroundAppsBtn.Click += CloseBackgroundAppsBtn_Click;
            actionsStack.Children.Add(closeBackgroundAppsBtn);
            
            var showActiveAppBtn = new Button 
            { 
                Content = "Show Active Application", 
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)), 
                Foreground = new SolidColorBrush(Colors.White), 
                Padding = new Thickness(15, 10, 15, 10), 
                Margin = new Thickness(0, 0, 0, 10) 
            };
            showActiveAppBtn.Click += ShowActiveAppBtn_Click;
            actionsStack.Children.Add(showActiveAppBtn);
            
            actionsGroup.Content = actionsStack;
            ContentPanel.Children.Add(actionsGroup);
            
            // Active Application Info
            var activeAppGroup = new GroupBox { Header = "Active Application", Margin = new Thickness(0, 0, 0, 20) };
            var activeAppStack = new StackPanel();
            
            var activeAppLabel = new TextBlock { x:Name = "ActiveAppText", Text = "Active Application: " + _booster.GetActiveApplicationName(), FontSize = 14, Margin = new Thickness(0, 0, 0, 10) };
            activeAppStack.Children.Add(activeAppLabel);
            
            activeAppGroup.Content = activeAppStack;
            ContentPanel.Children.Add(activeAppGroup);
        }

        private void LoadSettingsContent()
        {
            var settingsView = new SettingsView();
            ContentPanel.Children.Add(settingsView);
        }

        private void AddMetricRow(Grid grid, int row, string label, string value)
        {
            var labelBlock = new TextBlock { Text = label, FontSize = 14, FontWeight = "SemiBold", Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) };
            var valueBlock = new TextBlock { Text = value, FontSize = 14, Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)) };
            
            var labelBorder = new Border { Padding = new Thickness(10), Background = new SolidColorBrush(Color.FromRgb(236, 240, 241)), CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 5, 5) };
            labelBorder.Child = labelBlock;
            
            var valueBorder = new Border { Padding = new Thickness(10), Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)), CornerRadius = new CornerRadius(4), Margin = new Thickness(5, 0, 0, 5) };
            valueBorder.Child = valueBlock;
            
            var rowPanel = new StackPanel { Orientation = Orientation.Horizontal };
            rowPanel.Children.Add(labelBorder);
            rowPanel.Children.Add(valueBorder);
            
            Grid.SetRow(rowPanel, row);
            grid.Children.Add(rowPanel);
        }

        private async void OptimizeSystemBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Optimizing system...";
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;
            
            await Task.Run(() => _booster.OptimizeProcesses());
            
            StatusText.Text = "System optimization complete!";
            if (button != null) button.IsEnabled = true;
        }

        private async void IntelligentOptimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Performing intelligent optimization...";
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;
            
            await Task.Run(() => _booster.IntelligentOptimize());
            
            StatusText.Text = "Intelligent optimization complete!";
            if (button != null) button.IsEnabled = true;
        }

        // Methods that can be called from DashboardView
        public async void OptimizeSystemFromDashboard()
        {
            StatusText.Text = "Optimizing system...";
            await Task.Run(() => _booster.OptimizeProcesses());
            StatusText.Text = "System optimization complete!";
            UpdateSystemMetrics(); // Refresh metrics after optimization
        }

        public async void IntelligentOptimizeFromDashboard()
        {
            StatusText.Text = "Performing intelligent optimization...";
            await Task.Run(() => _booster.IntelligentOptimize());
            StatusText.Text = "Intelligent optimization complete!";
            UpdateSystemMetrics(); // Refresh metrics after optimization
        }

        private void CleanMemoryBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Cleaning system memory...";
            
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            StatusText.Text = "Memory cleaned successfully!";
        }

        private void ReduceWorkingSetBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Reducing working set...";
            
            // On Windows, try to reduce working set size
            _booster.GetType().GetMethod("ReduceWorkingSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(_booster, null);
            
            StatusText.Text = "Working set reduced!";
        }

        private void CleanupBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Performing system cleanup...";
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;

            Task.Run(async () =>
            {
                var result = await _cleanup.QuickCleanupAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusText.Text = result.Message;
                    if (button != null) button.IsEnabled = true;
                });
            });
        }

        private void SecurityScanBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Performing security scan...";
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;

            Task.Run(() =>
            {
                var securityMetrics = _securityMonitor.GetSecurityMetrics();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusText.Text = $"Security scan complete. Threats found: {securityMetrics.Threats.Count}, Security Score: {securityMetrics.SecurityScore}";
                    if (button != null) button.IsEnabled = true;
                });
            });
        }

        private void HealthCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Performing system health check...";
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;

            Task.Run(async () =>
            {
                var healthResult = await _healthChecker.PerformHealthCheckAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusText.Text = $"Health check complete. Status: {healthResult.OverallStatus} (Score: {healthResult.HealthScore}/100)";
                    if (button != null) button.IsEnabled = true;
                });
            });
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateSystemMetrics();
            StatusText.Text = "Data refreshed";
        }

        private void StopMonitoringBtn_Click(object sender, RoutedEventArgs e)
        {
            _booster.StopMonitoring();
            _isMonitoring = false;
            StatusText.Text = "Monitoring stopped";
        }

        private async void CloseBackgroundAppsBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Closing background applications...";
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;

            try
            {
                await _booster.CloseBackgroundProcesses();
                StatusText.Text = "Background applications closed successfully!";
                
                // Update the active application display
                if (ActiveAppText != null)
                {
                    ActiveAppText.Text = "Active Application: " + _booster.GetActiveApplicationName();
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error closing background apps: {ex.Message}";
            }
            
            if (button != null) button.IsEnabled = true;
        }

        private void ShowActiveAppBtn_Click(object sender, RoutedEventArgs e)
        {
            string activeApp = _booster.GetActiveApplicationName();
            StatusText.Text = $"Active Application: {activeApp}";
            
            if (ActiveAppText != null)
            {
                ActiveAppText.Text = "Active Application: " + activeApp;
            }
        }

        private void UpdateCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Checking for updates...";
            _updateManager.CheckForUpdatesWithNotification();
        }

        private string GetOperatingSystemInfo()
        {
            var platform = Environment.OSVersion.Platform;
            var version = Environment.OSVersion.VersionString;
            
            if (platform == PlatformID.Win32NT)
            {
                return "Windows";
            }
            else if (platform == PlatformID.Unix)
            {
                return "Linux/Unix";
            }
            else if (platform == PlatformID.MacOSX)
            {
                return "macOS";
            }
            
            return platform.ToString();
        }

        protected override void OnClosed(EventArgs e)
        {
            _booster?.StopMonitoring();
            _updateTimer?.Stop();
            _updateManager?.Dispose();
            base.OnClosed(e);
        }
    }
}