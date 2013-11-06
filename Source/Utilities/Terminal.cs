using System;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace AbsoluteZero {

    /// <summary>
    /// Represents the terminal for engine input and output. 
    /// </summary>
    static class Terminal {

        /// <summary>
        /// The width of the terminal window. 
        /// </summary>
        private const Int32 TerminalWidth = 82;

        /// <summary>
        /// The height of the terminal window. 
        /// </summary>
        private const Int32 TerminalHeight = 25;

        /// <summary>
        /// The text that has been processed. 
        /// </summary>
        private static StringBuilder _text = new StringBuilder();

        /// <summary>
        /// Initializes the terminal. 
        /// </summary>
        public static void Initialize() {
            try {
                Console.Title = "Engine Terminal";
                Console.SetWindowSize(TerminalWidth, TerminalHeight);
            } catch { }
        }

        /// <summary>
        /// Writes the specified string to the standard output stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(String value) {
            _text.Append(value);
            Console.Write(value);
        }

        /// <summary>
        /// Writes the text representation of the given object to the standard output 
        /// stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(Object value) {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes the text representation of the given objects to the standard 
        /// output stream using the given formatting. 
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(string format, params Object[] values)
        {
            Write(String.Format(format, values));
        }

        /// <summary>
        /// Writes the given string, followed by the current line terminator, to the 
        /// standard output stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(String value = "") {
            _text.AppendLine(value);
            Console.WriteLine(value);
        }

        /// <summary>
        /// Writes the text representation of the given object, followed by the 
        /// current line terminator, to the standard output stream. 
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(Object value) {
            WriteLine(value.ToString());
        }

        /// <summary>
        /// Writes the text representation of the given objects, followed by the 
        /// current line terminator, to the standard output stream using the given 
        /// formatting. 
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(string format, params Object[] values)
        {
            WriteLine(String.Format(format, values));
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
                sw.Write(_text.ToString());
        }

        /// <summary>
        /// Hides the terminal window. 
        /// </summary>
        public static void Hide() {
            Native.ShowWindow(Native.GetConsoleWindow(), Native.SW_HIDE);
        }
    }
}
