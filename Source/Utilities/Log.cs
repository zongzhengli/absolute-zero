﻿using System;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace AbsoluteZero {

    /// <summary>
    /// The console for engine input and output. 
    /// </summary>
    static class EngineConsole {

        /// <summary>
        /// The width of the console window. 
        /// </summary>
        private const Int32 ConsoleWidth = 82;

        /// <summary>
        /// The height of the console window. 
        /// </summary>
        private const Int32 ConsoleHeight = 25;

        /// <summary>
        /// The text that has been processed. 
        /// </summary>
        private static StringBuilder text = new StringBuilder();

        /// <summary>
        /// Initializes the console. 
        /// </summary>
        public static void Initialize() {
            try {
                Console.Title = "Engine Console";
                Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            } catch { }
        }

        /// <summary>
        /// Writes the specified string to the standard output stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(String value) {
            text.Append(value);
            Console.Write(value);
        }

        /// <summary>
        /// Writes the text representation of the specified object string to the 
        /// standard output stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(Object value) {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes the specified string, followed by the current line terminator, to 
        /// the standard output stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(String value = "") {
            text.AppendLine(value);
            Console.WriteLine(value);
        }

        /// <summary>
        /// Writes the text representation of the specified object string, followed 
        /// by the current line terminator, to the standard output stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(Object value) {
            WriteLine(value.ToString());
        }

        /// <summary>
        /// Writes any buffered data to the underlying device. 
        /// </summary>
        public static void Flush() {
            Console.Out.Flush();
        }

        /// <summary>
        /// Writes all the text that has been written to the standard output stream 
        /// to a file with the specified path. 
        /// </summary>
        /// <param name="path">The path of the file to write to.</param>
        public static void SaveText(String path) {
            using (StreamWriter sw = new StreamWriter(path))
                sw.WriteLine(text.ToString());
        }

        /// <summary>
        /// Hides the console window. 
        /// </summary>
        public static void HideConsole() {
            Native.ShowWindow(Native.GetConsoleWindow(), Native.SW_HIDE);
        }
    }
}
