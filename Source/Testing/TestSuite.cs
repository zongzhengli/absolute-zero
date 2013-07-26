using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AbsoluteZero {
    static class TestSuite {
        private const Int32 ColumnWidth = 12;
        private const Int32 IDWidthLimit = ColumnWidth - 3;

        public static List<String> Parse(String[] parameters) {
            Restrictions.Reset();
            String fileName = String.Empty;
            for (Int32 i = 0; i < parameters.Length; i++)
                if (parameters[i].EndsWith(".epd"))
                    fileName = parameters[i];
                else
                    switch (parameters[i]) {
                        case "time":
                            Restrictions.MoveTime = Int32.Parse(parameters[i + 1]);
                            break;
                        case "depth":
                            Restrictions.Depth = Int32.Parse(parameters[i + 1]);
                            break;
                        case "nodes":
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

        public static void Run(String[] parameters) {
            new Thread(new ThreadStart(delegate {
                Execute(Parse(parameters));
            })) {
                IsBackground = true
            }.Start();
            Application.Run(new Window());
        }

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

                Int32 bmIndex = line.IndexOf("bm ");
                bmIndex = bmIndex < 0 ? Int32.MaxValue : bmIndex;
                Int32 amIndex = line.IndexOf("am ");
                amIndex = amIndex < 0 ? Int32.MaxValue : amIndex;
                String fen = line.Remove(Math.Min(bmIndex, amIndex));
                List<String> solutions = new List<String>();
                for (Int32 i = terms.IndexOf("bm") + 1; i >= 0 && i < terms.Count && terms[i] != ";"; i++)
                    solutions.Add(terms[i]);
                Int32 idIndex = line.IndexOf("id ") + 3;
                String id = line.Substring(idIndex, line.IndexOf(';', idIndex) - idIndex).Replace("\"", String.Empty);
                if (id.Length > IDWidthLimit)
                    id = id.Remove(IDWidthLimit) + "..";

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

                String result = "fail";
                if (solutions.Contains(Identify.MoveAlgebraically(position, move))) {
                    result = "pass";
                    totalSolved++;
                }
                Terminal.WriteLine(Format.PadRightAll(ColumnWidth, id, result, Format.Precision(elapsed) + " ms", engine.GetNodes()));
            }
            Terminal.WriteLine("-----------------------------------------------------------------------");
            Terminal.WriteLine("Result: " + totalSolved + " / " + totalPositions);
            Terminal.WriteLine("Time elapsed: " + Format.Precision(totalTime) + " ms");
            Terminal.WriteLine("Average nodes: " + Format.Precision(totalNodes / (Double)totalPositions));
        }
    }
}
