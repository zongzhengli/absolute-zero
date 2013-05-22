using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AbsoluteZero {
    static class Log {
        private const Int32 ConsoleWidth = 82;
        private const Int32 ConsoleHeight = 25;

        private static List<String> text = new List<String>();

        public static void Initialize() {
            try {
                Console.Title = "Engine Output";
                Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            } catch { }
        }

        public static void WriteLine(String value = "") {
            text.Add(value);
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
                text.ForEach(line => {
                    sw.WriteLine(line);
                });
        }

        public static void HideConsole() {
            Native.ShowWindow(Native.GetConsoleWindow(), Native.SW_HIDE);
        }
    }
}
