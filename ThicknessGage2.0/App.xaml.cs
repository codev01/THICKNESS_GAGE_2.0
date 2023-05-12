using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

using ThicknessGage2_0.Common;
using ThicknessGage2_0.Utils;

namespace ThicknessGage2_0
{
    public partial class App : Application
    {
        private Mutex Mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            string assemblyName = ResourceAssembly.GetName().Name;
            Mutex = new Mutex(true, assemblyName);
            if (!Mutex.WaitOne(100))
            {
                foreach (var item in Process.GetProcessesByName(assemblyName))
                {
                    if (item.MainWindowHandle != IntPtr.Zero)
                    {
                        Win32.SetForegroundWindow(item.MainWindowHandle);
                    }
                }
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                Logger.Info("Приложение запущено");
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                Exit += App_Exit;
            }
            base.OnStartup(e);
        }


        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
            => Logger.Error(e.Exception);

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (Mutex != null)
                Mutex.ReleaseMutex();

            Logger.Info("Приложение закрыто");
            Logger.Close();
        }
    }
}
