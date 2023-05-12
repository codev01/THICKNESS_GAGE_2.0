using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows;

namespace ThicknessGage2_0.Properties
{
    public static class Configuration
    {
        public static short RestartTime_Seconds { get; private set; }

        public static bool IsSendingMessages { get; private set; }
        public static string SocketHost { get; private set; }
        public static int SocketPort { get; private set; }

        public static string OracleDataSource { get; private set; }
        public static string OracleUser { get; private set; }
        public static string OraclePassword { get; private set; }
        public static string OraclePackage { get; private set; }
        public static bool OracleWriteToDataBase { get; private set; }

        public static short Protocol_Telegram_Length { get; private set; }
        public static short CoilID_MSG_Length { get; private set; }
        public static short CoilID_MSG_Offset { get; private set; }

        static Configuration() => ReadConfiguration();

        public static void ReadConfiguration()
        {
            NameValueCollection valueCollection = ConfigurationManager.AppSettings;
            try
            {
                ConfigurationManager.RefreshSection("appSettings");

                RestartTime_Seconds = short.Parse(valueCollection.Get("RestartTime_Seconds"));

                IsSendingMessages = bool.Parse(valueCollection.Get("IsSendingMessages"));
                SocketHost = valueCollection.Get("SocketHost");
                SocketPort = int.Parse(valueCollection.Get("SocketPort"));

                OracleUser = valueCollection.Get("OracleUser");
                OraclePassword = valueCollection.Get("OraclePassword");
                OraclePackage = valueCollection.Get("OraclePackage");
                OracleDataSource = valueCollection.Get("OracleDataSource");
                OracleWriteToDataBase = bool.Parse(valueCollection.Get("OracleWriteToDataBase"));

                Protocol_Telegram_Length = short.Parse(valueCollection.Get("Protocol_Telegram_Length"));
                CoilID_MSG_Length = short.Parse(valueCollection.Get("CoilID_MSG_Length"));
                CoilID_MSG_Offset = short.Parse(valueCollection.Get("CoilID_MSG_Offset"));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString() + "\n\nИсправьте читаемые параметры конфигурации или верните их в исходнеое состояние",
                                "Ошибка при чтении файла конфигурации!",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

    }
}
