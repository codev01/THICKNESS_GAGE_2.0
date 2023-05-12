using System;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using ThicknessGage2_0.Properties;

namespace ThicknessGage2_0
{
    partial class MainWindow
    {
        public event Action OnRestart;

        /// <summary>
        /// Таймер на одну секунду в бесконечном цикле до вызова Stop()
        /// </summary>
        private Timer timerOneSec = new Timer()
        {
            Interval = new TimeSpan(0, 0, 1).TotalMilliseconds,
            AutoReset = true
        };
        private short restartTime => (short)new TimeSpan(0, 0, Configuration.RestartTime_Seconds).Seconds;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            string versionOut(int vNum)
            {
                return vNum != 0 ? $".{vNum}" : string.Empty;
            }
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Title += $" v{v.Major}.{v.Minor}{versionOut(v.Revision)}{versionOut(v.Build)}";
            restartBlock.Visibility = Visibility.Collapsed;
            msgIndex.Text = 0.ToString();
            restartIndex.Text = string.Empty;
            rootTextBlock.Text = string.Empty;
            configuration_TelegramLength.Text = string.Empty;
            configuration_CoilID_MSG_Length.Text = string.Empty;
            configuration_CoilID_MSG_Offset.Text = string.Empty;

            timerOneSec.Elapsed += RestartTick;
        }

        #region Update

        #region Indicators

        /// <summary>
        /// Получает цвет индикатора
        /// </summary>
        /// <param name="is">
        /// true: Зелёный
        /// /
        /// false: Красный
        /// </param>
        /// <returns>
        /// Цвет индикатора
        /// </returns>
        private Brush GetBoolToBrush(bool @is)
            => @is ? Brushes.Green : Brushes.Red;

        /// <summary>
        /// Обновляет цвет индикатора "Запись в базу"
        /// </summary>
        public void UpdateUI_indicator_WriteDataBase(bool @is)
            => TaskUI(() => indicator_WriteToDataBase.Fill = GetBoolToBrush(@is));

        /// <summary>
        /// Обновляет цвет индикатора "Отправка сообщений"
        /// </summary>
        public void UpdateUI_indicator_SendingMessages(bool @is)
            => TaskUI(() => indicator_SendingMessages.Fill = GetBoolToBrush(@is));

        /// <summary>
        /// Обновляет цвет индикатора "Получаемые сообщения протокольной длины"
        /// </summary>
        public void UpdateUI_indicator_TelegramLength(bool @is)
            => TaskUI(() => indicator_TelegramLength.Fill = GetBoolToBrush(@is));

        #endregion

        #region Configurations

        /// <summary>
        /// Обновляет протокольную длину байт телеграммы
        /// </summary>
        public void UpdateUI_configuration_TelegramLength(short value)
            => TaskUI(() => configuration_TelegramLength.Text = value.ToString());

        /// <summary>
        /// Обновляет протокольную длину байт COILID
        /// </summary>
        public void UpdateUI_configuration_CoilID_MSG_Length(short value)
            => TaskUI(() => configuration_CoilID_MSG_Length.Text = value.ToString());

        /// <summary>
        /// Обновляет протокльное смещение байт COILID
        /// </summary>
        public void UpdateUI_configuration_CoilID_MSG_Offset(short value)
            => TaskUI(() => configuration_CoilID_MSG_Offset.Text = value.ToString());

        #endregion

        #region Common

        /// <summary>
        /// Получает состояние отображения элемента
        /// </summary>
        /// <param name="is">
        /// true: Показать
        /// /
        /// false: Скрыть
        /// </param>
        /// <returns>
        /// Состояние отображения элемента
        /// </returns>
        private Visibility GetBoolToVisibility(bool @is)
            => @is ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Обновляет содержимое <see cref="rootTextBlock"/>
        /// </summary>
        private void UpdateUI_rootTextBlock(string value)
            => TaskUI(() => rootTextBlock.Text = value);

        /// <summary>
        /// Обновляет содержимое кнопки Start/Stop
        /// </summary>
        /// <param name="is">
        /// true: Stop
        /// /
        /// false: Start
        /// </param>
        public void UpdateUI_s_button_Content(bool @is)
            => TaskUI(() =>
            {
                s_button.IsChecked = @is;
                s_button.Content = @is ? "Stop" : "Start";
            });

        /// <summary>
        /// Обновляет индекс сообщения
        /// </summary>
        /// <param name="value">
        /// Индекс сообщения
        /// </param>
        public void UpdateUI_indexMsg(long value)
            => TaskUI(() => msgIndex.Text = value.ToString());

        /// <summary>
        /// Обновляет время обратного отсчёта до перезапуска
        /// </summary>
        /// <param name="value">
        /// Время в секундах
        /// </param>
        public void UpdateUI_tickIndex(short value)
            => TaskUI(() => restartIndex.Text = value.ToString());

        /// <summary>
        /// Изменяет видимость элемента обратного счётчика
        /// </summary>
        public void RestartInfoVisibility(bool @is)
            => TaskUI(() =>
            {
                restartBlock.Visibility = GetBoolToVisibility(@is);
                s_button.IsEnabled = !@is;
            });

        #endregion

        #endregion

        /// <summary>
        /// Позволяет обращаться к потоку пользовательского интерфейся из другого потока
        /// </summary>
        /// <param name="action">
        /// Блок кода требующий выполнения в потоке пользовательского интерфейса
        /// </param>
        private void TaskUI(Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Render)
            => Dispatcher.BeginInvoke(dispatcherPriority, action);

        /// <summary>
        /// Отображает обратный отсчёт до перезапуска и запускает таймер перезапуска
        /// </summary>
        public void Restart()
        {
            timerOneSec_tickIndex = restartTime;
            RestartTick(null, null);
            RestartInfoVisibility(true);
            timerOneSec.Start();
        }

        /// <summary>
        /// Хранит индекс собщений
        /// </summary>
        private long indexMsg = 0;
        /// <summary>
        /// Увеличивает индекс сообщений на один и выводит в окно
        /// </summary>
        private void Set_indexMsg()
            => UpdateUI_indexMsg(++indexMsg);

        /// <summary>
        /// Хранит индекс счётчика до перезапуска
        /// </summary>
        private short timerOneSec_tickIndex;
        /// <summary>
        /// Срабатывает каждую секунду после старта таймера и выводит оставшееся время до рестарта
        /// </summary>
        private void RestartTick(object sender, ElapsedEventArgs e)
        {
            if (timerOneSec_tickIndex != 0)
            {
                UpdateUI_tickIndex(timerOneSec_tickIndex--);
            }
            else
            {
                RestartInfoVisibility(false);
                timerOneSec.Stop();
                OnRestart?.Invoke();
            }
        }
    }
}
