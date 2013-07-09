using System;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace AbsoluteZero {
    static class Log {
        private const Int32 ConsoleWidth = 82;
        private const Int32 ConsoleHeight = 25;

        private static StringBuilder text = new StringBuilder();

        public static void Initialize() {
            try {
                Console.Title = "Engine Output";
                Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            } catch { }
        }

        public static void Write(String value) {
            text.Append(value);
            Console.Write(value);
        }

        public static void Write(Object value) {
            Write(value.ToString());
        }

        public static void WriteLine(String value = "") {
            text.AppendLine(value);
            Console.WriteLine(value);
        }

        public static void WriteLine(Object value) {
            WriteLine(value.ToString());
        }

        public static void Flush() {
            Console.Out.Flush();
        }

        public static void SaveText(String path) {
            using (StreamWriter sw = new StreamWriter(path))
                sw.WriteLine(text.ToString());
        }

        public static void HideConsole() {
            Native.ShowWindow(Native.GetConsoleWindow(), Native.SW_HIDE);
        }
    }
}
