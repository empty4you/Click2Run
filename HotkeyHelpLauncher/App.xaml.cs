using Hardcodet.Wpf.TaskbarNotification;
using HotkeyLauncher;
using System.Windows;

namespace HotkeyHelpLauncher
{
    public partial class App : Application
    {
        private TaskbarIcon trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            
            if (trayIcon != null)
            {
                trayIcon.Dispose();
                trayIcon = null;
            }

            base.OnExit(e);
        }

        
        private void OpenApp_Click(object sender, RoutedEventArgs e)    // открытие в трее
        {
            var win = Current.MainWindow;
            if (win == null)
            {
                win = new MainWindow();
                Current.MainWindow = win;
            }

            win.Show();
            win.WindowState = WindowState.Normal;
            win.Activate();
        }

        
        private void ExitApp_Click(object sender, RoutedEventArgs e) //закрытие в трее
        {
            Shutdown(); 
        }

        
        private void TrayIcon_DoubleClick(object sender, RoutedEventArgs e)
        {
            OpenApp_Click(sender, e);
        }
    }
}
