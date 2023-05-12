using System;
using System.Runtime.InteropServices;

namespace ThicknessGage2_0.Common
{
    public static class Win32
    {
        /// <summary>
        /// Выносит поток, создавший указанное окно, на передний план и активирует окно. 
        /// Ввод с клавиатуры направлен в окно, и для пользователя изменяются различные визуальные подсказки. 
        /// Система назначает потоку, создавшему окно переднего плана, чуть более высокий приоритет, чем другим потокам.
        /// <para/>
        /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow">docs.microsoft.com</see>
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);
    }
}
