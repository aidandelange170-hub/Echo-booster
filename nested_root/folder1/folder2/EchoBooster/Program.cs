using System;
using System.Windows;
using System.Windows.Threading;

namespace EchoBooster
{
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var application = new App();
            var mainWindow = new MainWindow();
            application.Run(mainWindow);
        }
    }
}