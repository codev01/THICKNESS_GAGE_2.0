using System;
using System.Diagnostics;
using System.Timers;

using ThicknessGage2_0.Properties;

namespace ThicknessGage2_0.Utils
{
    public static class DeleteLogs
    {
        public static Timer TimerBatDeleteLogs { get; }

        static DeleteLogs()
        {
            TimerBatDeleteLogs = new Timer()
            {
                AutoReset = true,
                Interval = new TimeSpan(1, 0, 0, 0).TotalMilliseconds
            };
            TimerBatDeleteLogs.Elapsed += TimerStartBatDelete_Elapsed;
        }

        private static void TimerStartBatDelete_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.Close();
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + Resources.NameScriptFile_delete_logs_14day);
            Logger.Initialize();
        }
    }
}
