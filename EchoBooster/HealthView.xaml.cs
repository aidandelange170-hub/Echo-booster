using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Threading.Tasks;

namespace EchoBooster
{
    public partial class HealthView : UserControl
    {
        private SystemHealthChecker _healthChecker;
        private SystemBooster _booster;

        public HealthView(SystemHealthChecker healthChecker, SystemBooster booster)
        {
            InitializeComponent();
            _healthChecker = healthChecker;
            _booster = booster;
            LoadHealthData();
        }

        private async void LoadHealthData()
        {
            var healthResult = await _healthChecker.PerformHealthCheckAsync();
            
            HealthScoreText.Text = healthResult.OverallStatus;
            IssuesCountText.Text = healthResult.Issues.Count.ToString();
            CriticalIssuesText.Text = healthResult.Issues.FindAll(i => 
                i.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase) || 
                i.Severity.Equals("High", StringComparison.OrdinalIgnoreCase)).Count.ToString();
            LastCheckedText.Text = healthResult.CheckedAt.ToString("HH:mm:ss");
            
            IssuesDataGrid.ItemsSource = healthResult.Issues;
        }

        private void HealthCheckBtn_Click(object sender, RoutedEventArgs e)
        {
            // This will be handled by the main window
            if (this.Parent is StackPanel panel && panel.Parent is ScrollViewer scroll && scroll.Parent is UserControl userControl)
            {
                Window parentWindow = Window.GetWindow(userControl);
                if (parentWindow is MainWindow mainWindow)
                {
                    mainWindow.HealthCheckBtn_Click(sender, e);
                }
            }
        }

        private async void GenerateReportBtn_Click(object sender, RoutedEventArgs e)
        {
            var report = await _healthChecker.GenerateHealthReportAsync();
            
            // Show the report in a message box or save to file
            MessageBox.Show(report, "System Health Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}