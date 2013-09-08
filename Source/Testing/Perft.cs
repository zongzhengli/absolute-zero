using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbsoluteZero {

    /// <summary>
    /// Provides methods for performing perft-related functions. 
    /// </summary>
    static class Perft {

        /// <summary>
        /// The maximum depth for perft. 
        /// </summary>
        private const Int32 DepthLimit = 64;

        /// <summary>
        /// The maximum number of moves to anticipate for a position. 
        /// </summary>
        private const Int32 MovesLimit = 256;

        /// <summary>
        /// Stores the generated moves. 
        /// </summary>
        private static Int32[][] _moves = new Int32[DepthLimit][];

        /// <summary>
        /// Initializes the moves array. 
        /// </summary>
        static Perft() {
            for (Int32 i = 0; i < _moves.Length; i++)
                _moves[i] = new Int32[MovesLimit];
        }

        /// <summary>
        /// Performs perft on the given position from a depth of 1 to the given 
        /// depth. This method writes the results to the terminal. 
        /// </summary>
        /// <param name="position">The position to perform perft on.</param>
        /// <param name="depth">The final depth to perform perft to.</param>
        public static void Iterate(Position position, Int32 depth) {
            const Int32 DepthWidth = 10;
            const Int32 TimeWidth = 11;
            const Int32 SpeedWidth = 14;
            String formatString = "{0,-" + DepthWidth + "}{1,-" + TimeWidth + "}{2,-" + SpeedWidth + "}{3}";

            Terminal.WriteLine(String.Format(formatString, "Depth", "Time", "Speed", "Nodes"));
            Terminal.WriteLine("-----------------------------------------------------------------------");
            for (Int32 d = 1; d <= depth; d++) {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Int64 nodes = Nodes(position, d);
                stopwatch.Stop();

                Double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                String t = Format.Precision(elapsed) + " ms";
                String s = Format.Precision(nodes / elapsed) + " kN/s";

                Terminal.WriteLine(String.Format(formatString, d, t, s, nodes));
            }
            Terminal.WriteLine("-----------------------------------------------------------------------");
        }

        /// <summary>
        /// Performs divide on the given position with the given depth. This
        /// essentially performs perft on each of the positions arising from the 
        /// legal moves for the given position. This method writes the results to 
        /// the terminal. 
        /// </summary>
        /// <param name="position">The position to perform divide on.</param>
        /// <param name="depth">The depth to perform divide with.</param>
        public static void Divide(Position position, Int32 depth) {
            const Int32 MoveWidth = 8;
            String formatString = "{0,-" + MoveWidth + "}{1}";

            Terminal.WriteLine(String.Format(formatString, "Move", "Nodes"));
            Terminal.WriteLine("-----------------------------------------------------------------------");
            Int64 totalNodes = 0;
            List<Int32> moves = position.LegalMoves();
            foreach (Int32 move in moves) {
                position.Make(move);
                Int64 nodes = Nodes(position, depth - 1);
                position.Unmake(move);
                totalNodes += nodes;

                Terminal.WriteLine(String.Format(formatString, Identify.Move(move), nodes));
            }
            Terminal.WriteLine("-----------------------------------------------------------------------");
            Terminal.WriteLine("Moves: " + moves.Count);
            Terminal.WriteLine("Nodes: " + totalNodes);
        }

        /// <summary>
        /// Performs perft on the given position with the given depth and returns the 
        /// result. 
        /// </summary>
        /// <param name="position">The position to perform perft on.</param>
        /// <param name="depth">The depth to perform perft with.</param>
        /// <returns>The result of performing perft.</returns>
        public static Int64 Nodes(Position position, Int32 depth) {
            if (depth <= 0)
                return 1;
            Int32 movesCount = position.LegalMoves(_moves[depth]);
            if (depth == 1)
                return movesCount;
            Int64 nodes = 0;
            for (Int32 i = 0; i < movesCount; i++) {
                position.Make(_moves[depth][i]);
                nodes += Nodes(position, depth - 1);
                position.Unmake(_moves[depth][i]);
            }
            return nodes;
        }

        /// <summary>
        /// Estimates the result of performing perft on the given position with the 
        /// given depth and returns the estimation. 
        /// </summary>
        /// <param name="position">The position to estimate performing perft on.</param>
        /// <param name="depth">The depth to estimate performing perft with.</param>
        /// <param name="milliseconds">The number of milliseconds given for the estimation.</param>
        /// <param name="epsilon">The factor giving the fraction of nodes to actually evaluate.</param>
        /// <returns>The estimated result of performing perft.</returns>
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

        /// <summary>
        /// Simulates the result of performing perft on the given position with the 
        /// given depth and returns the result. 
        /// </summary>
        /// <param name="position">The position to simulate performing perft on.</param>
        /// <param name="depth">The depth to simulate performing perft with.</param>
        /// <param name="epsilon">The factor giving the fraction of nodes to actually evaluate.</param>
        /// <returns>The simulated result of performing perft.</returns>
        private static Int64 Simulate(Position position, Int32 depth, Double epsilon) {
            Int32 movesCount = position.LegalMoves(_moves[depth]);
            if (depth <= 1)
                return Random.Double() < epsilon ? movesCount : 0;
            Int64 nodes = 0;
            for (Int32 i = 0; i < movesCount; i++)
                if (Random.Double() < epsilon) {
                    position.Make(_moves[depth][i]);
                    nodes += Simulate(position, depth - 1, epsilon);
                    position.Unmake(_moves[depth][i]);
                }
            return nodes;
        }

    }
}
