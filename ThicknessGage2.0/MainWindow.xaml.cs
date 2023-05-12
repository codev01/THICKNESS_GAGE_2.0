using System;
using System.Windows;

using ThicknessGage2_0.Client;
using ThicknessGage2_0.Properties;
using ThicknessGage2_0.Utils;

namespace ThicknessGage2_0
{
    public partial class MainWindow : Window
    {
        #region Fields

        public DataBase DataBase;
        public SocketClient SocketClient;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            OnRestart += OnRestarting;
            DataBase = new DataBase();
            SocketClient = new SocketClient(DataBase);
            DeleteLogs.TimerBatDeleteLogs.Start();
            UpdateConfigInfo();
            Start();
        }

        /// <summary>
        /// Срабатывает по нажатии на кнопку Read.config
        /// </summary>
        private void ReadConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.ReadConfiguration();
            UpdateConfigInfo();
        }

        /// <summary>
        /// Обновляет переменные и визуальные значения
        /// </summary>
        public void UpdateConfigInfo()
        {
            UpdateUI_configuration_TelegramLength(Configuration.Protocol_Telegram_Length);
            UpdateUI_configuration_CoilID_MSG_Length(Configuration.CoilID_MSG_Length);
            UpdateUI_configuration_CoilID_MSG_Offset(Configuration.CoilID_MSG_Offset);
        }

        /// <summary>
        /// Событе нажатия кнопки Start/Stop
        /// </summary>
        private void S_button_Click(object sender, RoutedEventArgs e)
        {
            if (s_button.IsChecked.Value)
                Start();
            else
                Stop();
        }

        public void Start()
        {
            UpdateUI_s_button_Content(true);
            DataBase.OnWriting += DataBase_OnWritingToDataBase;
            SocketClient.OnReceivedMessage += SocketClient_OnReceivedMessage;
            SocketClient.OnSentMessage += SocketClient_OnSentMessage;
            SocketClient.OnTaskMessagingFlowException += Exception;
            try
            {
                try
                {
                    DataBase.Connect();
                    SocketClient.Connect();
                }
                catch (Exception e)
                {
                    throw new Exception("Ошибка подключения", e);
                }
                SocketClient.Start();
            }
            catch (Exception e)
            {
                Exception(e);
            }
        }

        public void Stop()
        {
            UpdateUI_s_button_Content(false);
            DataBase.OnWriting -= DataBase_OnWritingToDataBase;
            SocketClient.OnReceivedMessage -= SocketClient_OnReceivedMessage;
            SocketClient.OnSentMessage -= SocketClient_OnSentMessage;
            SocketClient.OnTaskMessagingFlowException -= Exception;
            SocketClient.Stop();
        }

        /// <summary>
        /// Срабатывает после вызова метода <see cref="Restart()"/> через указанное время в <see cref="restartTime"/>
        /// </summary>
        private void OnRestarting()
        {
            Stop();
            Start();
        }

        private void SwitchOff()
        {
            UpdateUI_indicator_SendingMessages(false);
            UpdateUI_indicator_TelegramLength(false);
            UpdateUI_indicator_WriteDataBase(false);
        }

        private void Exception(Exception e)
        {
            SwitchOff();
            string exceptionStr = string.Empty;
            Exception exception = e;
            while (exception != null)
            {
                exceptionStr += $"{exception.Message}\n";
                exception = exception.InnerException;
            }

            UpdateUI_rootTextBlock(exceptionStr);
            Restart();
            Logger.Error(e);
        }

        private void DataBase_OnWritingToDataBase(bool @is)
            => UpdateUI_indicator_WriteDataBase(@is);

        private void SocketClient_OnReceivedMessage(string msg, bool isProtocol)
        {
            UpdateUI_indicator_TelegramLength(isProtocol);
            receivedStr = msg;
            OutStrMSG();
            Logger.Info(msg);
        }

        private void SocketClient_OnSentMessage(string msg, bool isSended)
        {
            sendedStr = msg;
            OutStrMSG();
            UpdateUI_indicator_SendingMessages(isSended);
            if (isSended && !string.IsNullOrEmpty(msg))
                Set_indexMsg();

            Logger.Info(msg);
        }

        private string receivedStr;
        private string sendedStr;
        private void OutStrMSG()
        {
            UpdateUI_rootTextBlock($"{receivedStr}\n{sendedStr}");
        }
    }
}
