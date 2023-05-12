using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows;

namespace ThicknessGage
{
    public static class Configuration
    {
        public static string SocketHost { get; private set; }
        public static int SocketPort { get; private set; }

        public static string OracleDataSource { get; private set; }
        public static string OracleUser { get; private set; }
        public static string OraclePassword { get; private set; }
        public static string OraclePackage { get; private set; }
        public static bool OracleWriteToDataBase { get; private set; }

        public static int CoilID_MSG_Length { get; private set; }
        public static int CoilID_MSG_Offset { get; private set; }

        public static void ReadConfiguration()
        {
            NameValueCollection valueCollection = ConfigurationManager.AppSettings;
            try
            {
                ConfigurationManager.RefreshSection("appSettings");

                SocketHost = valueCollection.Get("SocketHost");
                SocketPort = int.Parse(valueCollection.Get("SocketPort"));

                OracleUser = valueCollection.Get("OracleUser");
                OraclePassword = valueCollection.Get("OraclePassword");
                OraclePackage = valueCollection.Get("OraclePackage");
                OracleDataSource = valueCollection.Get("OracleDataSource");
                OracleWriteToDataBase = bool.Parse(valueCollection.Get("OracleWriteToDataBase"));

                CoilID_MSG_Length = int.Parse(valueCollection.Get("CoilID_MSG_Length"));
                CoilID_MSG_Offset = int.Parse(valueCollection.Get("CoilID_MSG_Offset"));
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
