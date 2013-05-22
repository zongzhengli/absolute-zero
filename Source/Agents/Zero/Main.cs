using System;
using System.Collections.Generic;

namespace AbsoluteZero {
    partial class Zero : IEngine {
        public Int32 GetMove(Position position) {
            if (Restrictions.Output == OutputType.Standard) {
                Log.WriteLine(Format.PadRight("Depth", DepthWidth) + Format.PadRight("Value", ValueWidth) + "Principal Variation");
                Log.WriteLine("-----------------------------------------------------------------------");
            }

            Prepare();
            stopwatch.Reset();
            stopwatch.Start();
            Int32 move = SearchRoot(position);
            stopwatch.Stop();

            if (Restrictions.Output == OutputType.Standard) {
                Log.WriteLine("-----------------------------------------------------------------------");
                Double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                Log.WriteLine("FEN: " + position.GetFEN());
                Log.WriteLine();
                Log.WriteLine(position.ToStringAppend(
                    "Absolute Zero " + 8 * IntPtr.Size + "-bit",
                    "Version " + Version,
                    String.Empty,
                    "Nodes visited: " + totalNodes,
                    "Search time: " + Format.Precision(elapsed) + " ms",
                    "Search speed: " + Format.Precision(totalNodes / elapsed) + " kN/s",
                    String.Empty,
                    "Quiescence nodes: " + Format.Precision(100D * quiescenceNodes / totalNodes, 2) + "%",
                    "Hash usage: " + Format.Precision(100D * table.Count / table.Capacity, 2) + "%",
                    "Hash cutoffs: " + Format.Precision(100D * hashCutoffs / hashProbes, 2) + "%",
                    "Static evaluation: " + Format.PrecisionAndSign(.01 * Evaluate(position), 2)
                    ));
                Log.WriteLine();
            }
            return move;
        }

        private String OutputString(Position position, Int32 depth, Int32 value, List<Int32> pv) {
            Boolean isMate = Math.Abs(value) > NearCheckmateValue;
            Int32 movesToMate = (CheckmateValue - Math.Abs(value) + 1) / 2;

            switch (Restrictions.Output) {
                case OutputType.Standard:
                    String depthString = Format.PadRight(depth, DepthWidth);
                    String valueString = Format.PrecisionAndSign(.01 * value, 2);
                    if (isMate)
                        valueString = (value > 0 ? "+Mate " : "-Mate ") + movesToMate;
                    valueString = Format.PadRight(valueString, ValueWidth);
                    String movesString = Identify.MovesAlgebraically(position, pv);
                    return depthString + valueString + movesString;
                case OutputType.Universal:
                    String score = "cp " + value;
                    if (isMate)
                        score = "mate " + (value < 0 ? "-" : String.Empty) + movesToMate;
                    Double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                    Int64 nps = (Int64)(1000 * totalNodes / elapsed);
                    return "info depth " + depth + " score " + score + " time " + (Int32)elapsed + " nodes " + totalNodes + " nps " + nps + " pv " + Identify.Moves(pv);
            }
            return String.Empty;
        }

        private List<Int32> CollectPV(Position position, Int32 depth, Int32 firstMove) {
            List<Int32> variation = new List<Int32>(depth);
            variation.Add(firstMove);
            for (Int32 i = 0; i < pvLength[1]; i++)
                variation.Add(pvMoves[1][i]);
            return variation;
        }

        public List<Int32> GetPV() {
            return pv;
        }

        public Int64 GetNodes() {
            return totalNodes;
        }

        public void AllocateHash(Int32 megabytes) {
            if (megabytes != table.Size >> 20)
                table = new HashTable(megabytes);
        }

        public void Stop() {
            abortSearch = true;
        }

        public void Reset() {
            table.Clear();
            for (Int32 i = 0; i < killerMoves.Length; i++)
                Array.Clear(killerMoves[i], 0, killerMoves[i].Length);
            finalAlpha = 0;
            rootAlpha = 0;
        }

        public Boolean AcceptDraw() {
            return finalAlpha <= DrawValue;
        }

        public String GetName() {
            return "Absolute Zero " + Version;
        }

        private void Prepare() {
            abortSearch = false;
            totalNodes = 0;
            quiescenceNodes = 0;
            referenceNodes = 0;
            hashProbes = 0;
            hashCutoffs = 0;
        }
    }
}
