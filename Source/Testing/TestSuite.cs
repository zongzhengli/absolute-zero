using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Provides methods for evaluating engine analysis on test suites. 
    /// </summary>
    static class TestSuite {

        /// <summary>
        /// The string that determines output formatting. 
        /// </summary>
        private static readonly String ResultFormat = String.Format("{{0,-{0}}}{{1,-{0}}}{{2,-{0}}}{{3}}", ColumnWidth);

        /// <summary>
        /// The number of characters in a column for output. 
        /// </summary>
        private const Int32 ColumnWidth = 12;

        /// <summary>
        /// The maximum number of characters that are displayed for the ID of a test 
        /// position. 
        /// </summary>
        private const Int32 IDWidthLimit = ColumnWidth - 3;

        /// <summary>
        /// Begins the test with the given positions. 
        /// </summary>
        /// <param name="epd">A list of positions in EPD format.</param>
        public static void Run(List<String> epd) {

            // Perform testing on a background thread. 
            new Thread(new ThreadStart(() => {
                IEngine engine = new Zero();
                Restrictions.Output = OutputType.None;
                Int32 totalPositions = 0;
                Int32 totalSolved = 0;
                Int64 totalNodes = 0;
                Double totalTime = 0;

                Terminal.WriteLine(ResultFormat, "Position", "Result", "Time", "Nodes");
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
                    String id = line.Substring(idIndex, line.IndexOf(';', idIndex) - idIndex).Replace(@"\", "");
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
                    totalNodes += engine.Nodes;

                    // Determine whether the engine found a solution. 
                    String result = "fail";
                    if (solutions.Contains(Stringify.MoveAlgebraically(position, move))) {
                        result = "pass";
                        totalSolved++;
                    }

                    // Print the result for the search on the position. 
                    Terminal.WriteLine(ResultFormat, id, result, String.Format("{0:0} ms", elapsed), engine.Nodes);
                }

                // Print final results after all positions have been searched. 
                Terminal.WriteLine("-----------------------------------------------------------------------");
                Terminal.WriteLine("Result         {0} / {1}", totalSolved, totalPositions);
                Terminal.WriteLine("Time           {0:0} ms", totalTime);
                Terminal.WriteLine("Average nodes  {0:0}", (Double)totalNodes / totalPositions);
            })) {
                IsBackground = true
            }.Start();

            // Open the GUI window to draw positions. 
            Application.Run(new Window());
        }
    }
}
