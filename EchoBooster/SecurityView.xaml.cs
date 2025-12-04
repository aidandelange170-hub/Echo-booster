using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EchoBooster
{
    public partial class SecurityView : UserControl
    {
        private SecurityMonitor _securityMonitor;
        private SystemBooster _booster;

        public SecurityView(SecurityMonitor securityMonitor, SystemBooster booster)
        {
            InitializeComponent();
            _securityMonitor = securityMonitor;
            _booster = booster;
            LoadSecurityData();
        }

        private void LoadSecurityData()
        {
            var securityMetrics = _securityMonitor.GetSecurityMetrics();
            
            SecurityScoreText.Text = securityMetrics.SecurityScore;
            ThreatsCountText.Text = securityMetrics.Threats.Count.ToString();
            SuspiciousProcessesText.Text = securityMetrics.SuspiciousProcesses.ToString();
            AntivirusStatusText.Text = securityMetrics.AntivirusRunning ? "Active" : "Inactive";
            
            ThreatsDataGrid.ItemsSource = securityMetrics.Threats;
        }

        private void SecurityScanBtn_Click(object sender, RoutedEventArgs e)
        {
            // This will be handled by the main window
            if (this.Parent is StackPanel panel && panel.Parent is ScrollViewer scroll && scroll.Parent is UserControl userControl)
            {
                Window parentWindow = Window.GetWindow(userControl);
                if (parentWindow is MainWindow mainWindow)
                {
                    mainWindow.SecurityScanBtn_Click(sender, e);
                }
            }
        }
    }
}