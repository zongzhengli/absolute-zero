using System;
using System.Collections.Generic;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the main interface of the Absolute Zero chess engine. 
    /// </summary>
    partial class Zero : IEngine {

        /// <summary>
        /// The principal variation of the most recent search. 
        /// </summary>
        public List<Int32> PrincipalVariation {
            get {
                return _pv;
            }
        }

        /// <summary>
        /// The number of nodes visited during the most recent search. 
        /// </summary>
        public Int64 Nodes {
            get {
                return _totalNodes;
            }
        }

        /// <summary>
        /// The size of the transposition table in megabytes. 
        /// </summary>
        public Int32 HashAllocation {
            get {
                return _table.Size >> 20;
            }
            set {
                if (value != _table.Size >> 20)
                    _table = new HashTable(value << 20);
            }
        }

        /// <summary>
        /// Whether to use experimental features. 
        /// </summary>
        public Boolean IsExperimental {
            get;
            set;
        }

        /// <summary>
        /// The name of the engine. 
        /// </summary>
        public String Name {
            get {
                return "Absolute Zero " + Version;
            }
        }

        /// <summary>
        /// Whether the engine is willing to accept a draw offer. 
        /// </summary>
        public Boolean AcceptsDraw {
            get {
                return _finalAlpha <= DrawValue;
            }
        }

        /// <summary>
        /// Returns the best move as determined by the engine. This method may write 
        /// output to the terminal. 
        /// </summary>
        /// <param name="position">The position to analyse.</param>
        /// <returns>The best move as determined by the engine.</returns>
        public Int32 GetMove(Position position) {
            if (Restrictions.Output == OutputType.Standard) {
                Terminal.WriteLine(PVFormat, "Depth", "Value", "Principal Variation");
                Terminal.WriteLine("-----------------------------------------------------------------------");
            }

            // Initialize variables to prepare for search. 
            _abortSearch = false;
            _totalNodes = 0;
            _quiescenceNodes = 0;
            _referenceNodes = 0;
            _hashProbes = 0;
            _hashCutoffs = 0;
            _hashMoveChecks = 0;
            _hashMoveMatches = 0;
            _killerMoveChecks = 0;
            _killerMoveMatches = 0;
            _futileMoves = 0;
            _movesSearched = 0;
            _stopwatch.Reset();
            _stopwatch.Start();

            // Perform the search. 
            Int32 move = Search(position);

            // Output search statistics. 
            _stopwatch.Stop();
            Double elapsed = _stopwatch.Elapsed.TotalMilliseconds;

            if (Restrictions.Output == OutputType.Standard) {
                Terminal.WriteLine("-----------------------------------------------------------------------");
                Terminal.WriteLine("FEN: " + position.GetFEN());
                Terminal.WriteLine();
                Terminal.WriteLine(position.ToString(
                    String.Format("Absolute Zero {0} ({1}-bit)", Version, IntPtr.Size * 8),
                    String.Format("Search time        {0:0} ms", elapsed),
                    String.Format("Search speed       {0:0} kN/s", _totalNodes / elapsed),
                    String.Format("Nodes visited      {0}", _totalNodes),
                    String.Format("Moves processed    {0}", _movesSearched),
                    String.Format("Quiescence nodes   {0:0.00 %}", (Double)_quiescenceNodes / _totalNodes),
                    String.Format("Futility moves     {0:0.00 %}", (Double)_futileMoves / _movesSearched),
                    String.Format("Hash cutoffs       {0:0.00 %}", (Double)_hashCutoffs / _hashProbes),
                    String.Format("Hash move rate     {0:0.00 %}", (Double)_hashMoveMatches / _hashMoveChecks),
                    String.Format("Killer moves rate  {0:0.00 %}", (Double)_killerMoveMatches / _killerMoveChecks),
                    String.Format("Static evaluation  {0:+0.00;-0.00}", Evaluate(position) / 100.0)));
                Terminal.WriteLine();
            }
            return move;
        }

        /// <summary>
        /// Stops the search if applicable. 
        /// </summary>
        public void Stop() {
            _abortSearch = true;
        }

        /// <summary>
        /// Resets the engine. 
        /// </summary>
        public void Reset() {
            _table.Clear();
            for (Int32 i = 0; i < _killerMoves.Length; i++)
                Array.Clear(_killerMoves[i], 0, _killerMoves[i].Length);
            _finalAlpha = 0;
            _rootAlpha = 0;
            _totalNodes = 0;
        }
    }
}
