using System;
using System.Collections.Generic;
using System.Text;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the search component of the Absolute Zero chess engine. 
    /// </summary>
    partial class Zero {

        /// <summary>
        /// Returns the best move for the given position as determined by an 
        /// iterative deepening search framework. This is the main entry point for 
        /// the search algorithm. 
        /// </summary>
        /// <param name="position">The position to search on.</param>
        /// <returns>The predicted best move.</returns>
        private Int32 Search(Position position) {

            // Generate legal moves. Return immediately if there is only one legal move 
            // when playing with time controls. 
            List<Int32> moves = position.LegalMoves();
            if (Restrictions.UseTimeControls && moves.Count <= 1)
                return moves[0];

            // Initialize variables to prepare for search. 
            Int32 colour = position.SideToMove;
            Int32 depthLimit = Math.Min(DepthLimit, Restrictions.Depth);
            _timeLimit = Restrictions.MoveTime;
            _timeExtension = 0;

            // Allocate search time when playing with time controls. 
            if (Restrictions.UseTimeControls) {
                Double timeAllocation = Restrictions.TimeControl[colour] / Math.Max(20, Math.Ceiling(60 * Math.Exp(-0.007 * position.HalfMoves)));
                _timeLimit = timeAllocation + Restrictions.TimeIncrement[colour] - TimeControlsExpectedLatency;
                _timeExtensionLimit = 0.3 * Restrictions.TimeControl[colour] - timeAllocation;
            }

            // Apply iterative deepening. The search is repeated with incrementally 
            // higher depths until it is terminated. 
            for (Int32 depth = 1; depth <= depthLimit; depth++) {
                Int32 alpha = -Infinity;

                // Go through the move list. 
                for (Int32 i = 0; i < moves.Count; i++) {
                    _movesSearched++;

                    Int32 value = alpha + 1;
                    Int32 move = moves[i];
                    Boolean causesCheck = position.CausesCheck(move);
                    position.Make(move);

                    // Apply principal variation search with aspiration windows. The first move 
                    // is searched with a window centered around the best value found from the 
                    // most recent preceding search. If the result does not lie within the 
                    // window, a re-search is initiated with an open window. 
                    if (i == 0) {
                        Int32 lower = _rootAlpha - AspirationWindow;
                        Int32 upper = _rootAlpha + AspirationWindow;

                        value = -Search(position, depth - 1, 1, -upper, -lower, causesCheck);
                        if (value <= lower || value >= upper) {
                            TryTimeExtension(TimeControlsResearchThreshold, TimeControlsResearchExtension);
                            value = -Search(position, depth - 1, 1, -Infinity, Infinity, causesCheck);
                        }
                    }

                    // Subsequent moves are searched with a zero window search. If the result is 
                    // better than the best value so far, a re-search is initiated with a wider 
                    // window.
                    else {
                        value = -Search(position, depth - 1, 1, -alpha - 1, -alpha, causesCheck);
                        if (value > alpha)
                            value = -Search(position, depth - 1, 1, -Infinity, -alpha, causesCheck);
                    }

                    // Unmake the move and check for search termination. 
                    position.Unmake(move);
                    if (_abortSearch)
                        goto exit;

                    // Check for new best move. If the current move has the best value so far, 
                    // it is moved to the front of the list. This ensures the best move is 
                    // always the first move in the list, also gives a rough ordering of the 
                    // moves, and so subsequent searches are more efficient. The principal 
                    // variation is collected at this point. 
                    if (value > alpha) {
                        alpha = _rootAlpha = value;
                        moves.RemoveAt(i);
                        moves.Insert(0, move);
                        PrependPV(move, 0);

                        // Output principal variation for high depths. This happens on every depth 
                        // increase and every time an improvement is found. 
                        if (Restrictions.Output != OutputType.None && depth > SingleVariationDepth)
                            Terminal.WriteLine(GetPVString(position, depth, alpha, GetPrincipalVariation()));
                    }
                }

                // Output principal variation for low depths. This happens once for every 
                // depth since improvements are very frequent. 
                if (Restrictions.Output != OutputType.None && depth <= SingleVariationDepth)
                    Terminal.WriteLine(GetPVString(position, depth, alpha, GetPrincipalVariation()));

                // Check for early search termination. If there is no time extension and a 
                // significiant proportion of time has already been used, so that completing 
                // one more depth is unlikely, the search is terminated. 
                if (Restrictions.UseTimeControls && _timeExtension <= 0 && _stopwatch.ElapsedMilliseconds / _timeLimit > TimeControlsContinuationThreshold)
                    goto exit;
            }
        exit:
            _finalAlpha = _rootAlpha;
            return moves[0];
        }

        /// <summary>
        /// Returns the dynamic value of the position as determined by a recursive 
        /// search to the given depth. This implements the main search algorithm. 
        /// </summary>
        /// <param name="position">The position to search on.</param>
        /// <param name="depth">The depth to search to.</param>
        /// <param name="ply">The number of plies from the root position.</param>
        /// <param name="alpha">The lower bound on the value of the best move.</param>
        /// <param name="beta">The upper bound on the value of the best move.</param>
        /// <param name="inCheck">Whether the side to play is in check.</param>
        /// <param name="allowNull">Whether a null move is permitted.</param>
        /// <returns>The value of the termination position given optimal play.</returns>
        private Int32 Search(Position position, Int32 depth, Int32 ply, Int32 alpha, Int32 beta, Boolean inCheck, Boolean allowNull = true) {

            // Check whether to enter quiescence search and initialize pv length. 
            _pvLength[ply] = 0;
            if (depth <= 0 && !inCheck)
                return Quiescence(position, ply, alpha, beta);

            // Check for time extension and search termination. This is done once for 
            // every given number of nodes for efficency. 
            if (++_totalNodes > _referenceNodes) {
                _referenceNodes += NodeResolution;

                // Apply loss time extension. The value of the best move for the current 
                // root position is compared with the value of the previous root position. 
                // If there is a large loss, a time extension is given. 
                Int32 loss = _finalAlpha - _rootAlpha;
                if (loss >= TimeControlsLossResolution) {
                    Int32 index = Math.Min(loss / TimeControlsLossResolution, TimeControlsLossExtension.Length - 1);
                    TryTimeExtension(TimeControlsLossThreshold, TimeControlsLossExtension[index]);
                }

                if (_stopwatch.ElapsedMilliseconds >= _timeLimit + _timeExtension || _totalNodes >= Restrictions.Nodes)
                    _abortSearch = true;
            }
            if (_abortSearch)
                return Infinity;

            // Perform draw detection. 
            Int32 drawValue = ((ply & 1) == 0) ? DrawValue : -DrawValue;
            Int32 drawRepetitions = (ply > 2) ? 2 : 3;
            if (position.FiftyMovesClock >= 100 || position.InsufficientMaterial() || position.HasRepeated(drawRepetitions))
                return drawValue;

            // Perform mate distance pruning. 
            Int32 mateAlpha = Math.Max(alpha, -(CheckmateValue - ply));
            Int32 mateBeta = Math.Min(beta, CheckmateValue - (ply + 1));
            if (mateAlpha >= mateBeta)
                return mateAlpha;

            // Perform hash probe. 
            _hashProbes++;
            Int32 hashMove = Move.Invalid;
            HashEntry hashEntry;

            if (_table.TryProbe(position.ZobristKey, out hashEntry)) {
                hashMove = hashEntry.Move;
                if (hashEntry.Depth >= depth) {
                    Int32 hashType = hashEntry.Type;
                    Int32 hashValue = hashEntry.GetValue(ply);
                    if ((hashType == HashEntry.Beta && hashValue >= beta) || (hashType == HashEntry.Alpha && hashValue <= alpha)) {
                        _hashCutoffs++;
                        return hashValue;
                    }
                }
            }

            Int32 colour = position.SideToMove;

            // Apply null move heuristic. 
            if (allowNull && !inCheck && position.Bitboard[colour] != (position.Bitboard[colour | Piece.King] | position.Bitboard[colour | Piece.Pawn])) {
                position.MakeNull();
                Int32 reduction = NullMoveReduction + (depth >= NullMoveAggressiveDepth ? depth / NullMoveAggressiveDivisor : 0);
                Int32 value = -Search(position, depth - 1 - reduction, ply + 1, -beta, -beta + 1, false, false);
                position.UnmakeNull();
                if (value >= beta)
                    return value;
            }

            // Generate legal moves and perform basic move ordering. 
            Int32[] moves = _generatedMoves[ply];
            Int32 movesCount = position.LegalMoves(moves);
            if (movesCount == 0)
                return inCheck ? -(CheckmateValue - ply) : drawValue;
            for (Int32 i = 0; i < movesCount; i++)
                _moveValues[i] = MoveOrderingValue(moves[i]);

            // Apply single reply and check extensions. 
            if (movesCount == 1 || inCheck) 
                depth++;

            // Perform killer move ordering. 
            _killerMoveChecks++;
            bool killerMoveFound = false;
            for (Int32 slot = 0; slot < KillerMovesAllocation; slot++) {
                Int32 killerMove = _killerMoves[ply][slot];
                for (Int32 i = 0; i < movesCount; i++) {
                    if (moves[i] == killerMove) {
                        _moveValues[i] = KillerMoveValue + slot * KillerMoveSlotValue;
                        if (!killerMoveFound)
                            _killerMoveMatches++;
                        killerMoveFound = true;
                        break;
                    }
                }
            }

            // Perform hash move ordering. 
            _hashMoveChecks++;
            if (hashMove != Move.Invalid) {
                for (Int32 i = 0; i < movesCount; i++) {
                    if (moves[i] == hashMove) {
                        _moveValues[i] = HashMoveValue;
                        _hashMoveMatches++;
                        break;
                    }
                }
            }

            // Check for futility pruning activation. 
            Boolean futileNode = false;
            Int32 futilityValue = 0;
            if (depth < FutilityMargin.Length && !inCheck) {
                futilityValue = Evaluate(position) + FutilityMargin[depth];
                futileNode = futilityValue <= alpha;
            }

            // Sort the moves based on their ordering values and initialize variables. 
            Sort(moves, _moveValues, movesCount);
            Int32 irreducibleMoves = 1;
            while (irreducibleMoves < movesCount && _moveValues[irreducibleMoves] > 0)
                irreducibleMoves++;
            UInt64 preventionBitboard = PassedPawnPreventionBitboard(position);
            Int32 bestType = HashEntry.Alpha;
            Int32 bestMove = moves[0];

            // Go through the move list. 
            for (Int32 i = 0; i < movesCount; i++) {
                _movesSearched++;

                Int32 move = moves[i];
                Boolean causesCheck = position.CausesCheck(move);
                Boolean dangerous = inCheck || causesCheck || alpha < -NearCheckmateValue || IsDangerousPawnAdvance(move, preventionBitboard);
                Boolean reducible = i + 1 > irreducibleMoves;

                // Perform futility pruning. 
                if (futileNode && !dangerous && futilityValue + PieceValue[Move.Capture(move)] <= alpha) {
                    _futileMoves++;
                    continue;
                }

                // Make the move and initialize its value. 
                position.Make(move);
                Int32 value = alpha + 1;

                // Perform late move reductions. 
                if (reducible && !dangerous)
                    value = -Search(position, depth - 1 - LateMoveReduction, ply + 1, -alpha - 1, -alpha, causesCheck);

                // Perform principal variation search.
                else if (i > 0)
                    value = -Search(position, depth - 1, ply + 1, -alpha - 1, -alpha, causesCheck);

                // Perform a full search.
                if (value > alpha) 
                    value = -Search(position, depth - 1, ply + 1, -beta, -alpha, causesCheck);

                // Unmake the move and check for search termination. 
                position.Unmake(move);
                if (_abortSearch)
                    return Infinity;

                // Check for upper bound cutoff. 
                if (value >= beta) {
                    _table.Store(new HashEntry(position, depth, ply, move, value, HashEntry.Beta));
                    if (reducible) {
                        for (Int32 j = _killerMoves[ply].Length - 2; j >= 0; j--)
                            _killerMoves[ply][j + 1] = _killerMoves[ply][j];
                        _killerMoves[ply][0] = move;
                    }
                    return value;
                }

                // Check for lower bound improvement. 
                if (value > alpha) {
                    alpha = value;
                    bestMove = move;
                    bestType = HashEntry.Exact;

                    // Collect the principal variation. 
                    _pvMoves[ply][0] = move;
                    for (Int32 j = 0; j < _pvLength[ply + 1]; j++)
                        _pvMoves[ply][j + 1] = _pvMoves[ply + 1][j];
                    _pvLength[ply] = _pvLength[ply + 1] + 1;
                }
            }

            // Store the results in the hash table and return the lower bound of the 
            // value of the position. 
            _table.Store(new HashEntry(position, depth, ply, bestMove, alpha, bestType));
            return alpha;
        }

        /// <summary>
        /// Returns the dynamic value of the position as determined by a recursive 
        /// search that terminates upon reaching a quiescent position. 
        /// </summary>
        /// <param name="position">The position to search on.</param>
        /// <param name="ply">The number of plies from the root position.</param>
        /// <param name="alpha">The lower bound on the value of the best move.</param>
        /// <param name="beta">The upper bound on the value of the best move.</param>
        /// <returns>The value of the termination position given optimal play.</returns>
        private Int32 Quiescence(Position position, Int32 ply, Int32 alpha, Int32 beta) {
            _totalNodes++;
            _quiescenceNodes++;

            // Evaluate the position statically. Check for upper bound cutoff and lower 
            // bound improvement. 
            Int32 value = Evaluate(position);
            if (value >= beta)
                return value;
            if (value > alpha)
                alpha = value;

            // Initialize variables and generate the pseudo-legal moves to be 
            // considered. Perform basic move ordering and sort the moves. 
            Int32 colour = position.SideToMove;
            Int32[] moves = _generatedMoves[ply];
            Int32 movesCount = position.PseudoQuiescenceMoves(moves);
            for (Int32 i = 0; i < movesCount; i++)
                _moveValues[i] = MoveOrderingValue(moves[i]);
            Sort(moves, _moveValues, movesCount);

            // Go through the move list. 
            for (Int32 i = 0; i < movesCount; i++) {
                _movesSearched++;
                Int32 move = moves[i];

                // Consider the move only if it doesn't immediately lose material. This 
                // improves efficiency. 
                if ((Move.Piece(move) & Piece.Mask) <= (Move.Capture(move) & Piece.Mask) || EvaluateStaticExchange(position, move) >= 0) {

                    // Make the move. 
                    position.Make(move);

                    // Search the move if it is legal. This is equivalent to not leaving the 
                    // king in check. 
                    if (!position.InCheck(colour)) {
                        value = -Quiescence(position, ply + 1, -beta, -alpha);

                        // Check for upper bound cutoff and lower bound improvement. 
                        if (value >= beta) {
                            position.Unmake(move);
                            return value;
                        }
                        if (value > alpha)
                            alpha = value;
                    }

                    // Unmake the move. 
                    position.Unmake(move);
                }
            }
            return alpha;
        }

        /// <summary>
        /// Returns a string that describes the given principal variation. 
        /// </summary>
        /// <param name="position">The position the principal variation is to be played on.</param>
        /// <param name="depth">The depth of the search that yielded the principal variation.</param>
        /// <param name="value">The value of the search that yielded the principal variation.</param>
        /// <param name="pv">The principle variation to describe.</param>
        /// <returns>A string that describes the given principal variation.</returns>
        private String GetPVString(Position position, Int32 depth, Int32 value, List<Int32> pv) {
            Boolean isMate = Math.Abs(value) > NearCheckmateValue;
            Int32 movesToMate = (CheckmateValue - Math.Abs(value) + 1) / 2;

            switch (Restrictions.Output) {

                // Return standard output. 
                case OutputType.Standard:
                    String depthString = depth.ToString();
                    String valueString = isMate ? (value > 0 ? "+Mate " : "-Mate ") + movesToMate :
                                                  (value / 100.0).ToString("+0.00;-0.00");
                    String movesString = Stringify.MovesAlgebraically(position, pv);

                    return String.Format(PVFormat, depthString, valueString, movesString);

                // Return UCI output. 
                case OutputType.Universal:
                    String score = isMate ? "mate " + (value < 0 ? "-" : "") + movesToMate :
                                            "cp " + value;
                    Double elapsed = _stopwatch.Elapsed.TotalMilliseconds;
                    Int64 nps = (Int64)(1000 * _totalNodes / elapsed);

                    return String.Format("info depth {0} score {1} time {2} nodes {3} nps {4} pv {5}", depth, score, (Int32)elapsed, _totalNodes, nps, Stringify.Moves(pv));
            }
            return null;
        }

        /// <summary>
        /// Prepends the given move to the principal variation at the given ply.
        /// </summary>
        /// <param name="move">The move to prepend to the principal variation.</param>
        /// <param name="ply">The ply the move was made at.</param>
        private void PrependPV(Int32 move, Int32 ply) {
            _pvMoves[ply][0] = move;
            for (Int32 j = 0; j < _pvLength[ply + 1]; j++)
                _pvMoves[ply][j + 1] = _pvMoves[ply + 1][j];
            _pvLength[ply] = _pvLength[ply + 1] + 1;
        }

        /// <summary>
        /// Attempts to apply the time extension given. The time extension is applied 
        /// when playing under time controls if it is longer than the existing time 
        /// extension and if the proportion of time elapsed to the total time 
        /// allotted is greater than the given threshold. 
        /// </summary>
        /// <param name="threshold">The ratio between time elapsed and time allotted needed to trigger the time extension.</param>
        /// <param name="coefficient">The proportion of time allotted to extend by.</param>
        private void TryTimeExtension(Double threshold, Double coefficient) {
            Double newExtension = Math.Min(coefficient * _timeLimit, _timeExtensionLimit);
            if (Restrictions.UseTimeControls && newExtension > _timeExtension && _stopwatch.ElapsedMilliseconds / _timeLimit > threshold)
                _timeExtension = newExtension;
        }

        /// <summary>
        /// Returns whether the given move is a dangerous pawn advance. A dangerous 
        /// pawn advance is a pawn move that results in the pawn being in a position 
        /// in which no enemy pawns can threaten or block it. 
        /// </summary>
        /// <param name="move">The move to consider.</param>
        /// <param name="passedPawnPreventionBitboard">A bitboard giving the long term attack possibilities of the enemy pawns.</param>
        /// <returns>Whether the given move is a dangerous pawn advance.</returns>
        private Boolean IsDangerousPawnAdvance(Int32 move, UInt64 passedPawnPreventionBitboard) {
            return Move.IsPawnAdvance(move) && ((1UL << Move.To(move)) & passedPawnPreventionBitboard) == 0;
        }

        /// <summary>
        /// Returns a value for the given move that indicates its immediate threat. 
        /// Non-capture moves have a default value of zero, while captures have a 
        /// value that is the ratio of the captured piece to the moving piece. Pawns 
        /// promoting to queen are given an additional increase in value. 
        /// </summary>
        /// <param name="move">The move to consider.</param>
        /// <returns>A value for the given move that is useful for move ordering.</returns>
        private Single MoveOrderingValue(Int32 move) {
            Single value = PieceValue[Move.Capture(move)] / (Single)PieceValue[Move.Piece(move)];
            if (Move.IsQueenPromotion(move))
                value += QueenPromotionMoveValue;
            return value;
        }

        /// <summary>
        /// Returns a bitboard giving the long term attack possibilities of the enemy pawns. 
        /// </summary>
        /// <param name="position">The position to consider.</param>
        /// <returns>A bitboard giving the longer term attack possibilites of the enemy pawns.</returns>
        private static UInt64 PassedPawnPreventionBitboard(Position position) {
            UInt64 pawnblockBitboard = position.Bitboard[(1 - position.SideToMove) | Piece.Pawn];
            if (position.SideToMove == Colour.White) {
                pawnblockBitboard |= pawnblockBitboard << 8;
                pawnblockBitboard |= pawnblockBitboard << 16;
                pawnblockBitboard |= pawnblockBitboard << 32;
                pawnblockBitboard |= (pawnblockBitboard & NotAFileBitboard) << 7;
                pawnblockBitboard |= (pawnblockBitboard & NotHFileBitboard) << 9;
            } else {
                pawnblockBitboard |= pawnblockBitboard >> 8;
                pawnblockBitboard |= pawnblockBitboard >> 16;
                pawnblockBitboard |= pawnblockBitboard >> 32;
                pawnblockBitboard |= (pawnblockBitboard & NotAFileBitboard) >> 9;
                pawnblockBitboard |= (pawnblockBitboard & NotHFileBitboard) >> 7;
            }
            return pawnblockBitboard;
        }

        /// <summary>
        /// Sorts the given array of moves based on the given array of values. 
        /// </summary>
        /// <param name="moves">The array of moves to sort.</param>
        /// <param name="values">The array of values to sort.</param>
        /// <param name="count">The number of elements to sort.</param>
        private static void Sort(Int32[] moves, Single[] values, Int32 count) {
            for (Int32 i = 1; i < count; i++) {
                for (Int32 j = i; j > 0 && values[j] > values[j - 1]; j--) {
                    Single tempValue = values[j - 1];
                    values[j - 1] = values[j];
                    values[j] = tempValue;
                    Int32 tempMove = moves[j - 1];
                    moves[j - 1] = moves[j];
                    moves[j] = tempMove;
                }
            }
        }
    }
}
