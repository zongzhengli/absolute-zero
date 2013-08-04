using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Facilitates engine analysis on test suites. 
    /// </summary>
    static class TestSuite {

        /// <summary>
        /// The number of characters in a column. 
        /// </summary>
        private const Int32 ColumnWidth = 12;

        /// <summary>
        /// The maximum number of characters that are displayed for the ID of a test 
        /// position. 
        /// </summary>
        private const Int32 IDWidthLimit = ColumnWidth - 3;

        /// <summary>
        /// Parses the given parameters to determine engine restrictions and returns 
        /// a list of positions in EPD form. 
        /// </summary>
        /// <param name="parameters">Command-line parameters giving the position suite and engine restrictions. </param>
        /// <returns>A list of positions in EPD form.</returns>
        public static List<String> Parse(String[] parameters) {
            Restrictions.Reset();
            String fileName = String.Empty;
            for (Int32 i = 0; i < parameters.Length; i++)
                if (parameters[i].EndsWith(".epd"))
                    fileName = parameters[i];
                else
                    switch (parameters[i]) {
                        case "-t":
                            Restrictions.MoveTime = Int32.Parse(parameters[i + 1]);
                            break;
                        case "-d":
                            Restrictions.Depth = Int32.Parse(parameters[i + 1]);
                            break;
                        case "-n":
                            Restrictions.Nodes = Int32.Parse(parameters[i + 1]);
                            break;
                    }

            List<String> epd = new List<String>();
            using (StreamReader sr = new StreamReader(fileName))
                while (!sr.EndOfStream) {
                    String line = sr.ReadLine();
                    if (line.Length > 0)
                        epd.Add(line);
                }
            return epd;
        }

        /// <summary>
        /// Begins the test with the given parameters. 
        /// </summary>
        /// <param name="parameters">Command-line parameters giving the conditions of the test. </param>
        public static void Run(String[] parameters) {
            new Thread(new ThreadStart(delegate {
                Execute(Parse(parameters));
            })) {
                IsBackground = true
            }.Start();
            Application.Run(new Window());
        }

        /// <summary>
        /// Facilitates the test with the given positions. 
        /// </summary>
        /// <param name="epd">List of positions in the test suite in EPD form. </param>
        private static void Execute(List<String> epd) {
            IEngine engine = new Zero();
            Restrictions.Output = OutputType.None;
            Int32 totalPositions = 0;
            Int32 totalSolved = 0;
            Int64 totalNodes = 0;
            Double totalTime = 0;

            Terminal.WriteLine(Format.PadRightAll(ColumnWidth, "Position", "Result", "Time", "Nodes"));
            Terminal.WriteLine("-----------------------------------------------------------------------");
            foreach (String line in epd) {
                List<String> terms = new List<String>(line.Replace(";", " ;").Split(' '));

                // Strip everything to get the FEN. 
                Int32 bmIndex = line.IndexOf("bm ");
                bmIndex = bmIndex < 0 ? Int32.MaxValue : bmIndex;
                Int32 amIndex = line.IndexOf("am ");
                amIndex = amIndex < 0 ? Int32.MaxValue : amIndex;
                String fen = line.Remove(Math.Min(bmIndex, amIndex));

                // Get the best moves. 
                List<String> solutions = new List<String>();
                for (Int32 i = terms.IndexOf("bm") + 1; i >= 0 && i < terms.Count && terms[i] != ";"; i++)
                    solutions.Add(terms[i]);

                // Get the ID of the position. 
                Int32 idIndex = line.IndexOf("id ") + 3;
                String id = line.Substring(idIndex, line.IndexOf(';', idIndex) - idIndex).Replace("\"", String.Empty);
                if (id.Length > IDWidthLimit)
                    id = id.Remove(IDWidthLimit) + "..";

                // Set the position and invoke a search on it. 
                Position position = new Position(fen);
                VisualPosition.Set(position);
                engine.Reset();

                Stopwatch stopwatch = Stopwatch.StartNew();
                Int32 move = engine.GetMove(position);
                stopwatch.Stop();

                Double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                totalPositions++;
                totalTime += elapsed;
                totalNodes += engine.GetNodes();

                // Determine whether the engine found a solution. 
                String result = "fail";
                if (solutions.Contains(Identify.MoveAlgebraically(position, move))) {
                    result = "pass";
                    totalSolved++;
                }

                // Print the result for the search on the position. 
                Terminal.WriteLine(Format.PadRightAll(ColumnWidth, id, result, Format.Precision(elapsed) + " ms", engine.GetNodes()));
            }

            // Print final results after all positions have been searched. 
            Terminal.WriteLine("-----------------------------------------------------------------------");
            Terminal.WriteLine("Result: " + totalSolved + " / " + totalPositions);
            Terminal.WriteLine("Time elapsed: " + Format.Precision(totalTime) + " ms");
            Terminal.WriteLine("Average nodes: " + Format.Precision(totalNodes / (Double)totalPositions));
        }
    }
}
