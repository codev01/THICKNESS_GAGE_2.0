using System;

using NLog.Config;

using ThicknessGage2_0.Properties;

namespace ThicknessGage2_0.Utils
{
    public static class Logger
    {
        private static NLog.Logger logger;
        static Logger() 
            => Initialize();

        public static void Info(string message) 
            => logger.Info(message);

        public static void Error(Exception e) 
            => logger.Error(e);

        public static void Initialize()
        {
            NLog.LogManager.Configuration = new XmlLoggingConfiguration(Resources.PathOutDir_NLogConfig);
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public static void Close() 
            => NLog.LogManager.Shutdown();
    }
}
