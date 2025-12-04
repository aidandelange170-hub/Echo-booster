using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EchoBooster
{
    public partial class NetworkView : UserControl
    {
        private SystemBooster _booster;
        private ObservableCollection<NetworkConnectionInfo> _connections;
        
        public NetworkView(SystemBooster booster)
        {
            InitializeComponent();
            _booster = booster;
            _connections = new ObservableCollection<NetworkConnectionInfo>();
            NetworkConnectionsGrid.ItemsSource = _connections;
            
            LoadNetworkInfo();
        }
        
        private void LoadNetworkInfo()
        {
            var networkInfo = _booster.GetSystemMetrics().NetworkStatus;
            
            ActiveConnectionsText.Text = networkInfo.connections.ToString();
            NetworkStatusText.Text = networkInfo.status;
            NetworkSpeedText.Text = networkInfo.speed;
            
            LoadNetworkConnections();
        }
        
        private void LoadNetworkConnections()
        {
            _connections.Clear();
            
            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnections = properties.GetActiveTcpConnections();
                
                foreach (var connection in tcpConnections)
                {
                    var connectionInfo = new NetworkConnectionInfo
                    {
                        LocalAddress = $"{connection.LocalEndPoint.Address}:{connection.LocalEndPoint.Port}",
                        RemoteAddress = $"{connection.RemoteEndPoint.Address}:{connection.RemoteEndPoint.Port}",
                        Protocol = "TCP",
                        State = connection.State.ToString(),
                        ProcessName = GetProcessNameForPort(connection.LocalEndPoint.Port) // Simplified
                    };
                    
                    _connections.Add(connectionInfo);
                }
                
                // Add UDP listeners
                var udpListeners = properties.GetActiveUdpListeners();
                foreach (var endpoint in udpListeners)
                {
                    var connectionInfo = new NetworkConnectionInfo
                    {
                        LocalAddress = $"{endpoint.Address}:{endpoint.Port}",
                        RemoteAddress = "N/A",
                        Protocol = "UDP",
                        State = "Listening",
                        ProcessName = GetProcessNameForPort(endpoint.Port) // Simplified
                    };
                    
                    _connections.Add(connectionInfo);
                }
            }
            catch (Exception ex)
            {
                // Handle exception - in a real app you might show an error message
            }
        }
        
        private string GetProcessNameForPort(int port)
        {
            // This is a simplified implementation
            // In a real application, you would need to query the process associated with a specific port
            // which requires more complex system calls
            return "Unknown";
        }
    }
    
    public class NetworkConnectionInfo : INotifyPropertyChanged
    {
        private string _localAddress;
        private string _remoteAddress;
        private string _protocol;
        private string _state;
        private string _processName;
        
        public string LocalAddress
        {
            get { return _localAddress; }
            set { _localAddress = value; OnPropertyChanged(); }
        }
        
        public string RemoteAddress
        {
            get { return _remoteAddress; }
            set { _remoteAddress = value; OnPropertyChanged(); }
        }
        
        public string Protocol
        {
            get { return _protocol; }
            set { _protocol = value; OnPropertyChanged(); }
        }
        
        public string State
        {
            get { return _state; }
            set { _state = value; OnPropertyChanged(); }
        }
        
        public string ProcessName
        {
            get { return _processName; }
            set { _processName = value; OnPropertyChanged(); }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}