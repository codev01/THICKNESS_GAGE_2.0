using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using NLog;

namespace ThicknessGage
{
    public partial class MainWindow : Window
    {
#if DEBUG
        private const string socketHost = "10.96.108.21";
        private const int socketPort = 21;

#elif Redirector
        private const string socketHost = "10.96.163.1";
        private const int socketPort = 8001;
#else
        private string socketHost { get => Configuration.SocketHost; }
        private int socketPort { get => Configuration.SocketPort; }
#endif
        private bool isScrolled = false;
        private bool s_buttonIsChecked = false;
        private long indexLog = 0;
        private byte[] bytes = new byte[255];
        private Socket sender;
        private DataBase DataBase;
        private List<string> logs;
        private Timer timer;
        Logger logger;

        public MainWindow() => InitializeComponent();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            Title += $" v{assemblyName.Version}";

            Configuration.ReadConfiguration(); 

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "Log.txt"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
            logger = LogManager.GetCurrentClassLogger();

            timer = new Timer
            {
                AutoReset = false,
                Interval = 30000
            };
            timer.Elapsed += Timer_Elapsed;
            Closed += MainWindow_Closed;
            DataBase = new DataBase();
            Start();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            LogManager.Shutdown();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Start();
        }

        private void LogWriteLine(string text)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                   new Action(() => logText.Text += $"{text}\n"));
            logger.Info(text);
        }

        private void LogWrite(string text, bool isClear = false)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                   new Action(() =>
                                   {
                                       if (isClear)
                                           logText.Text = text;
                                       else
                                           logText.Text += text;
                                   }));
            logger.Info(text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!s_buttonIsChecked)
                Start();
            else
                Stop();
        }

        private void Start()
            => Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            s_buttonIsChecked = true;
                            s_button.Content = "Stop";
                            s_button.Background = Brushes.Red;
                            indicator.Background = Brushes.Green;
                            SocketClient().Start();
                        }));

        private void Stop()
            => Dispatcher.BeginInvoke(DispatcherPriority.Background,
                           new Action(() =>
                           {
                               s_buttonIsChecked = false;
                               s_button.Content = "Start";
                               s_button.Background = Brushes.Green;
                               indicator.Background = Brushes.Red;
                               StopClient();
                           }));

        private Task SocketClient()
        {
            logs = new List<string>(10);
            Task socketClientTask = null;
            try
            {
                socketClientTask = new Task(new Action(() =>
                {
                    LogWriteLine("Подключение к удалённому компьютеру.");
                    LogWriteLine($"IP: {socketHost}\n" +
                                 $"Port: {socketPort}");

                    IPAddress ipAddress = IPAddress.Parse(socketHost);
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, socketPort);
                    sender = new Socket(ipAddress.AddressFamily,
                                        SocketType.Stream,
                                        ProtocolType.Tcp);

                    try
                    {
                        try
                        {
                            var isDbConnected = DataBase.Connect();
                            LogWriteLine("Подключение к БД:" + isDbConnected.ToString());
                        }
                        catch
                        {
                            LogWriteLine("Ошибка подключения к  БД");
                            RestartSocketClient();
                        }

                        LogWriteLine("Идёт подключение, подождите...");
                        sender.Connect(remoteEP);
                        LogWriteLine("BINGO!!! (Connect)");
                        LogWriteLine("Socket connected to "
                            + sender.RemoteEndPoint.ToString());

                        Task task = new Task(() =>
                        {
                            LogWriteLine("Запущен поток обмена сообщениями");

                            while (s_buttonIsChecked)
                            {
                                string bottomContent = string.Empty;
                                string log = string.Empty;
                                if (sender.Connected)
                                {
                                    if (logs.Count >= 10)
                                    {
                                        logs.Remove(logs.First());
                                    }

                                    #region Ожидаем сообщение
                                    int bytesCount = 0;
                                    try
                                    {
                                        bytesCount = sender.Receive(bytes);
                                        if (bytesCount <= 0)
                                            throw new SocketException();
                                    }
                                    catch (SocketException e)
                                    {
                                        LogWrite("\nОжидание сообщений приостановлено\n" + e.Message);
                                        RestartSocketClient();
                                        break;
                                    }
                                    log += $"Принята телеграмма длиной {bytesCount} байт ({++indexLog}):\n";
                                    #endregion

                                    #region Оформляем полученную телеграмму
                                    string iMsg = string.Empty;
                                    for (int i = 0; i < bytesCount; i++)
                                    {
                                        iMsg += $"<{Convert.ToInt32(bytes[i])}>";
                                    }
                                    log += $"{iMsg}\n";
                                    #endregion

                                    #region Записываем полученную телеграмму в базу
#if !Redirector
                                    if (Configuration.OracleWriteToDataBase)
                                    {
                                        try
                                        {
                                            DataBase.SaveTo(iMsg, DateTime.Now);
                                            iMsg += "\nЗаписана в базу";
                                        }
                                        catch (Exception e)
                                        {
                                            bottomContent += "\nОшибка записи в базу:\n" + e.Message;
                                        }
                                    }
#endif
                                    #endregion

                                    #region Формируем ответную телеграмму и отправляем
                                    try
                                    {
                                        string coilId = DataBase.GetCoilIdZone();
                                        if (coilId.Length > 0 && coilId.Length < Configuration.CoilID_MSG_Length)
                                        {
                                            coilId += new string(' ', Configuration.CoilID_MSG_Length - coilId.Length);
                                            byte[] coilId_bytes = Encoding.UTF8.GetBytes(coilId);
                                            Array.Copy(coilId_bytes,
                                                       0,
                                                       bytes,
                                                       Configuration.CoilID_MSG_Offset,
                                                       coilId_bytes.Length);

                                            string oMsg = string.Empty;
                                            for (int i = 0; i < bytesCount; i++)
                                            {
                                                oMsg += $"<{Convert.ToInt32(bytes[i])}>";
                                            }
#if !Redirector
                                            sender.Send(bytes);
                                            oMsg += " (Отправлена)";
#endif
                                            log += $"Ответная телеграмма:\n{oMsg}\n";
                                        }
                                        else
                                        {
                                            bottomContent += $"Длина строки COILID = {coilId.Length}. " +
                                                          $"Значение COILID = {coilId} не соответствует протокольной = {Configuration.CoilID_MSG_Length}";
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        bottomContent += e.Message;
                                    }
                                    #endregion

                                    #region Вывод в окно
                                    string line = "--------------------";
                                    log += line + (indexLog % 4 == 0 ? '#' : '-') + line + "\n";

                                    logs.Add(log);
                                    string currentContent = string.Empty;
                                    foreach (var item in logs)
                                    {
                                        currentContent += item;
                                    }

                                    bottomContent += $"Состояние подключения к БД: {DataBase.GetConnectionState()}";

                                    LogWrite(currentContent + bottomContent, true);
                                    #endregion

                                    #region Автопрокрутка
                                    if (!isScrolled)
                                    {
                                        Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                            new Action(() =>
                                            {
                                                sv.ScrollToVerticalOffset(sv.ExtentHeight);
                                            }));
                                    }
                                    #endregion
                                }
                                else
                                {
                                    LogWriteLine("Отсутствует подключение сокета");
                                    RestartSocketClient();
                                    break;
                                }
                            }

                            LogWrite("\nВыход из потока обмена сообщениями");
                        });

                        task.Start();
                    }
                    catch (ArgumentNullException ane)
                    {
                        LogWriteLine("ArgumentNullException : " + ane.ToString());
                        RestartSocketClient();
                    }
                    catch (SocketException se)
                    {
                        LogWriteLine("SocketException : " + se.ToString());
                        RestartSocketClient();
                    }
                    catch (Exception e)
                    {
                        LogWriteLine("Unexpected exception : " + e.ToString());
                        RestartSocketClient();
                    }
                }));
            }
            catch
            {
                RestartSocketClient();
            }
            
            logs.Clear();
            return socketClientTask;
        }

        private void RestartSocketClient()
        {
            Stop();
            timer.Start();
        }

        private void StopClient()
        {
            if (sender.Connected)
            {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
        }

        private void sv_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isScrolled = true;
        }

        private void sv_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isScrolled = false;
        }

        private void sv_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        private void sv_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void Button_Click_ReadConfig(object sender, RoutedEventArgs e)
        {
            Configuration.ReadConfiguration();
        }

        private void Button_Click_ReconnectDB(object sender, RoutedEventArgs e)
        {
            DataBase.Connect();
        }
    }
}
