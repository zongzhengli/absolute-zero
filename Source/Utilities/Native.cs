using System;
using System.Runtime.InteropServices;

namespace AbsoluteZero {
    class Native {
        public const Int32 SW_HIDE = 0;

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);
    }
}
