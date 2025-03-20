using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    public static class WinAPIInvoke
    {
        [DllImport("kernel32.dll")]
        public static extern void OutputDebugString(string lpOutputString);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetTempFileName(
            string lpPathName,
            string lpPrefixString,
            uint uUnique,
            [Out] StringBuilder lpTempFileName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void HideConsole()
        {
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 0); //SW_HIDE;
            }
        }

        public static void ShowConsole()
        {
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 1); //SW_SHOWNORMAL
            }
        }
    }
}
