using System;
using System.Windows;
using System.Windows.Controls;

namespace EchoBooster
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            
            // Set up event handlers for sliders
            UpdateIntervalSlider.ValueChanged += UpdateIntervalSlider_ValueChanged;
            AutoOptimizeIntervalSlider.ValueChanged += AutoOptimizeIntervalSlider_ValueChanged;
            
            // Initialize slider text displays
            UpdateIntervalText.Text = $"{UpdateIntervalSlider.Value} seconds";
            AutoOptimizeIntervalText.Text = $"{AutoOptimizeIntervalSlider.Value} minutes";
        }

        private void UpdateIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateIntervalText.Text = $"{e.NewValue:F0} seconds";
        }

        private void AutoOptimizeIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AutoOptimizeIntervalText.Text = $"{e.NewValue:F0} minutes";
        }

        private void SaveSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would save settings to a configuration file
            MessageBox.Show("Settings saved successfully!", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            // Reset all settings to default values
            CpuMonitoringCheckBox.IsChecked = true;
            MemoryMonitoringCheckBox.IsChecked = true;
            DiskMonitoringCheckBox.IsChecked = true;
            NetworkMonitoringCheckBox.IsChecked = true;
            
            UpdateIntervalSlider.Value = 2;
            UpdateIntervalText.Text = "2 seconds";
            
            AutoOptimizeCheckBox.IsChecked = false;
            AutoOptimizeIntervalSlider.Value = 30;
            AutoOptimizeIntervalText.Text = "30 minutes";
            
            LowOptimizationRadio.IsChecked = true;
            LightThemeRadio.IsChecked = true;
        }

        private void ApplySettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would apply settings immediately
            MessageBox.Show("Settings applied successfully!", "Settings Applied", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}