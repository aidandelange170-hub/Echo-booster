using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EchoBooster
{
    public partial class ProcessesView : UserControl
    {
        private SystemBooster _booster;
        private ObservableCollection<ProcessInfo> _processes;
        
        public ProcessesView(SystemBooster booster)
        {
            InitializeComponent();
            _booster = booster;
            _processes = new ObservableCollection<ProcessInfo>();
            ProcessesDataGrid.ItemsSource = _processes;
            
            LoadProcesses();
        }
        
        private void LoadProcesses()
        {
            _processes.Clear();
            
            var processes = _booster.GetProcessList();
            foreach (var process in processes)
            {
                try
                {
                    var processInfo = new ProcessInfo
                    {
                        ProcessName = process.ProcessName,
                        ProcessId = process.ProcessId,
                        CpuUsage = process.CpuUsage,
                        MemoryUsage = process.MemoryUsage,
                        ThreadCount = process.ThreadCount,
                        Status = process.IsCritical ? "Critical" : "Running"
                    };
                    
                    _processes.Add(processInfo);
                }
                catch
                {
                    // Skip processes that can't be accessed
                }
            }
        }
    }
    
    public class ProcessInfo : INotifyPropertyChanged
    {
        private string _processName;
        private int _processId;
        private double _cpuUsage;
        private long _memoryUsage;
        private int _threadCount;
        private string _status;
        
        public string ProcessName
        {
            get { return _processName; }
            set { _processName = value; OnPropertyChanged(); }
        }
        
        public int ProcessId
        {
            get { return _processId; }
            set { _processId = value; OnPropertyChanged(); }
        }
        
        public double CpuUsage
        {
            get { return _cpuUsage; }
            set { _cpuUsage = value; OnPropertyChanged(); }
        }
        
        public long MemoryUsage
        {
            get { return _memoryUsage; }
            set { _memoryUsage = value; OnPropertyChanged(); }
        }
        
        public int ThreadCount
        {
            get { return _threadCount; }
            set { _threadCount = value; OnPropertyChanged(); }
        }
        
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}