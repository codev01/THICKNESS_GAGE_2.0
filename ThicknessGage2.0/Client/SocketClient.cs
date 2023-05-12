using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using ThicknessGage2_0.Contracts;
using ThicknessGage2_0.Properties;

namespace ThicknessGage2_0.Client
{
    public class SocketClient
    {
        #region Congiguration FIELDS

#if DEBUG
        // Измени значения для отладки на своей машине
        private const string socketHost = "10.96.108.21";
        private const int socketPort = 21;

#elif Redirector
        private const string socketHost = "10.96.163.1";
        private const int socketPort = 8001;
#else
        private string socketHost => Configuration.SocketHost;
        private int socketPort => Configuration.SocketPort;
#endif

#if Redirector
        private bool isSendMSG = false;
#else
        private bool isSendMSG => Configuration.IsSendingMessages;
#endif

        #endregion

        public bool IsSendMSG => isSendMSG;
        public bool IsMessagingFlowWorking { get; private set; }
        public bool IsSendedMessage { get; private set; }
        public Socket Socket;
        public IDBContract DataBase;

        public event Action<string, bool> OnReceivedMessage;
        public event Action<string, bool> OnSentMessage;
        public event Action<Exception> OnTaskMessagingFlowException;

        private byte[] msgBytes;

        public SocketClient(IDBContract dataBase)
        {
            DataBase = dataBase;
            msgBytes = new byte[255];
        }

        public void Connect()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(socketHost);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, socketPort);
                Socket = new Socket(ipAddress.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);
                Socket.Connect(remoteEP);
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка подключения к удалённому компьютеру", e);
            }
        }

        public void Start()
        {
            IsMessagingFlowWorking = true;
            MessagingFlowAsync();
        }

        public void Stop()
        {
            IsMessagingFlowWorking = false;
            if (Socket != null && Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
        }

        private void MessagingFlowAsync()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (IsMessagingFlowWorking)
                    {
                        if (Socket.Connected)
                        {
                            int msgLength = default;
                            try
                            {
                                msgLength = ReceiveMSG(msgBytes);
                            }
                            catch (Exception e)
                            {
                                if (IsMessagingFlowWorking)
                                    throw e;

                                break;
                            }

                            string strFormatMsg = GetFormatStr(msgLength, msgBytes);
                            OnReceivedMessage?.Invoke($"Получено ({msgLength} байт):\n" + strFormatMsg,
                                                      msgLength == Configuration.Protocol_Telegram_Length);
                            DataBase.SaveTo(strFormatMsg, DateTime.Now);

                            string sendedMsg = string.Empty;
                            string coilId = DataBase.GetCoilIdZone();
                            if (coilId.Length > 0 && coilId.Length <= Configuration.CoilID_MSG_Length)
                            {
                                coilId += new string(' ', Configuration.CoilID_MSG_Length - coilId.Length);
                                byte[] coilId_bytes = Encoding.UTF8.GetBytes(coilId);
                                Array.Copy(coilId_bytes,
                                           0,
                                           msgBytes,
                                           Configuration.CoilID_MSG_Offset,
                                           coilId_bytes.Length);

                                if (IsSendMSG)
                                {
                                    SendMSG(msgBytes);
                                    IsSendedMessage = true;
                                    sendedMsg = $"Отправлено ({msgLength} байт):\n" + GetFormatStr(msgLength, msgBytes);
                                }
                                else
                                {
                                    IsSendedMessage = false;
#if Redirector
                                    string @out = "В конфигурации Redirector недопустима отправка сообщений";
#else
                                    string @out = "Измените необходимое значение в файле .config";
#endif
                                    sendedMsg = "Сообщение не отправлено. " + @out;
                                }

                            }
                            else
                            {
                                IsSendedMessage = false;
                                sendedMsg = $"Сообщение не отправлено, так как длина строки COILID = {coilId.Length}. "
                                    + $"Значение COILID = {coilId} не соответствует протокольной = {Configuration.CoilID_MSG_Length}";
                            }

                            OnSentMessage?.Invoke(sendedMsg, IsSendedMessage);
                        }
                    }
                }
                catch (Exception e)
                {
                    OnTaskMessagingFlowException?.Invoke(new Exception("Ошибка в потоке обмена сообщениями", e));
                }
            });
        }

        private int ReceiveMSG(byte[] buffer)
        {
            try
            {
                int msgLength = Socket.Receive(buffer);
                if (msgLength <= 0)
                    throw new Exception("Пустое сообщение");

                return msgLength;
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка во время ожидания сообщения", e);
            }
        }

        private void SendMSG(byte[] value)
        {
            try
            {
                Socket.Send(value);
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка при попытке отапрвить сообщение", e);
            }
        }

        private string GetFormatStr(int receiveLingth, byte[] value)
        {
            string outStr = string.Empty;
            for (int i = 0; i < receiveLingth; i++)
            {
                outStr += $"<{Convert.ToInt32(value[i])}>";
            }

            return outStr;
        }
    }
}
