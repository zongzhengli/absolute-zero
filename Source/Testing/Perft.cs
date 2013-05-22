using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbsoluteZero {
    static class Perft {
        private const Int32 DepthLimit = 64;
        private const Int32 MovesLimit = 256;

        public static UInt64[] StartingValue = { 1, 20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956, 2439530234167, 69352859712417, 2097651003696806, 62854969236701747, 1981066775000396239 };

        private static Int32[][] moves = new Int32[DepthLimit][];

        static Perft() {
            for (Int32 i = 0; i < moves.Length; i++)
                moves[i] = new Int32[MovesLimit];
        }

        public static void Iterate(Position position, Int32 depth) {
            const Int32 DepthWidth = 10;
            const Int32 TimeWidth = 11;
            const Int32 SpeedWidth = 14;
            Log.WriteLine(Format.PadRight("Depth", DepthWidth) + Format.PadRight("Time", TimeWidth) + Format.PadRight("Speed", SpeedWidth) + "Nodes");
            Log.WriteLine("-----------------------------------------------------------------------");
            for (Int32 d = 1; d <= depth; d++) {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Int64 nodes = Nodes(position, d);
                stopwatch.Stop();
                Double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                Log.WriteLine(
                    Format.PadRight(d, DepthWidth) +
                    Format.PadRight(Format.Precision(elapsed) + " ms", TimeWidth) +
                    Format.PadRight(Format.Precision(nodes / elapsed) + " kN/s", SpeedWidth) +
                    nodes
                    );
            }
            Log.WriteLine("-----------------------------------------------------------------------");
        }

        public static void Divide(Position position, Int32 depth) {
            const Int32 MoveWidth = 8;
            Log.WriteLine(Format.PadRight("Move", MoveWidth) + "Nodes");
            Log.WriteLine("-----------------------------------------------------------------------");
            Int64 totalNodes = 0;
            List<Int32> moves = position.LegalMoves();
            foreach (Int32 move in moves) {
                position.Make(move);
                Int64 nodes = Nodes(position, depth - 1);
                position.Unmake(move);
                totalNodes += nodes;
                Log.WriteLine(Format.PadRight(Identify.Move(move), MoveWidth) + nodes);
            }
            Log.WriteLine("-----------------------------------------------------------------------");
            Log.WriteLine("Moves: " + moves.Count);
            Log.WriteLine("Nodes: " + totalNodes);
        }

        public static Int64 Nodes(Position position, Int32 depth) {
            if (depth <= 0)
                return 1;
            Int32 movesCount = position.LegalMoves(moves[depth]);
            if (depth <= 1)
                return movesCount;
            Int64 nodes = 0;
            for (Int32 i = 0; i < movesCount; i++) {
                position.Make(moves[depth][i]);
                nodes += Nodes(position, depth - 1);
                position.Unmake(moves[depth][i]);
            }
            return nodes;
        }

        public static Double Estimate(Position position, Int32 depth, Int32 milliseconds = 100, Double epsilon = .045) {
            Int32 iterations = 0;
            Double total = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < milliseconds) {
                total += Simulate(position, depth, epsilon);
                iterations++;
            }
            return Math.Round(Math.Pow(epsilon, -depth) * total / iterations);
        }

        private static Int64 Simulate(Position position, Int32 depth, Double epsilon) {
            Int32 movesCount = position.LegalMoves(moves[depth]);
            if (depth <= 1)
                return Random.Double() < epsilon ? movesCount : 0;
            Int64 nodes = 0;
            for (Int32 i = 0; i < movesCount; i++)
                if (Random.Double() < epsilon) {
                    position.Make(moves[depth][i]);
                    nodes += Simulate(position, depth - 1, epsilon);
                    position.Unmake(moves[depth][i]);
                }
            return nodes;
        }

    }
}
