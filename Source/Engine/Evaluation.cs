using System;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the evaluation component of the Absolute Zero chess engine. 
    /// </summary>
    public sealed partial class Zero : IEngine {

        /// <summary>
        /// Returns the estimated value of the given position as determined by static 
        /// analysis. 
        /// </summary>
        /// <param name="position">The position to evaluate.</param>
        /// <returns>The estimated value of the position.</returns>
        private Int32 Evaluate(Position position) {
            UInt64[] bitboard = position.Bitboard;
            Single opening = position.GetPhase();
            Single endgame = 1 - opening;

            _pawnAttackBitboard[Colour.White] = (bitboard[Colour.White | Piece.Pawn] & NotAFileBitboard) >> 9
                                              | (bitboard[Colour.White | Piece.Pawn] & NotHFileBitboard) >> 7;
            _pawnAttackBitboard[Colour.Black] = (bitboard[Colour.Black | Piece.Pawn] & NotAFileBitboard) << 7
                                              | (bitboard[Colour.Black | Piece.Pawn] & NotHFileBitboard) << 9;
            Single totalValue = TempoValue;

            // Evaluate symmetric features (material, position, etc).
            for (Int32 colour = Colour.White; colour <= Colour.Black; colour++) {
                UInt64 targetBitboard = ~bitboard[colour] & ~_pawnAttackBitboard[1 - colour];
                UInt64 pawnBitboard = bitboard[colour | Piece.Pawn];
                UInt64 enemyPawnBitboard = bitboard[(1 - colour) | Piece.Pawn];
                UInt64 allPawnBitboard = pawnBitboard | enemyPawnBitboard;
                Int32 enemyKingSquare = Bit.Read(bitboard[(1 - colour) | Piece.King]);
                Single value = position.Value[colour];

                // Evaluate king. 
                Int32 square = Bit.Read(bitboard[colour | Piece.King]);
                value += opening * PawnNearKingValue * Bit.Count(PawnShieldBitboard[square] & pawnBitboard);
                
                if ((allPawnBitboard & Bit.File[square]) == 0)
                    value += opening * KingOnOpenFileValue;

                if (Position.File(square) > 0 && (allPawnBitboard & Bit.File[square - 1]) == 0)
                    value += opening * KingAdjacentToOpenFileValue;

                if (Position.File(square) < 7 && (allPawnBitboard & Bit.File[square + 1]) == 0)
                    value += opening * KingAdjacentToOpenFileValue;

                // Evaluate bishops. 
                UInt64 pieceBitboard = bitboard[colour | Piece.Bishop];
                _minorAttackBitboard[colour] = 0;

                if ((pieceBitboard & (pieceBitboard - 1)) != 0)
                    value += BishopPairValue;

                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += BishopPositionValue[colour][square];

                    UInt64 pseudoMoveBitboard = Attack.Bishop(square, position.OccupiedBitboard);
                    value += BishopMobilityValue[Bit.Count(targetBitboard & pseudoMoveBitboard)];
                    _minorAttackBitboard[colour] |= pseudoMoveBitboard;
                }

                // Evaluate knights. 
                pieceBitboard = bitboard[colour | Piece.Knight];
                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += endgame * KnightToEnemyKingSpatialValue[square][enemyKingSquare];

                    UInt64 pseudoMoveBitboard = Attack.Knight(square);
                    value += KnightMobilityValue[Bit.Count(targetBitboard & pseudoMoveBitboard)];
                    _minorAttackBitboard[colour] |= pseudoMoveBitboard;
                }

                // Evaluate queens. 
                pieceBitboard = bitboard[colour | Piece.Queen];
                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += endgame * QueenToEnemyKingSpatialValue[square][enemyKingSquare];
                }

                // Evaluate pawns.
                Int32 pawns = 0;
                pieceBitboard = bitboard[colour | Piece.Pawn];
                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    pawns++;

                    if ((ShortForwardFileBitboard[colour][square] & pawnBitboard) != 0)
                        value += DoubledPawnValue;

                    else if ((PawnBlockadeBitboard[colour][square] & enemyPawnBitboard) == 0)
                        value += PassedPawnValue + endgame * PassedPawnEndgamePositionValue[colour][square];

                    if ((ShortAdjacentFilesBitboard[square] & pawnBitboard) == 0)
                        value += IsolatedPawnValue;
                }
                value += (pawns == 0) ? PawnDeficiencyValue : pawns * endgame * PawnEndgameGainValue;

                // Evaluate pawn threat to enemy minor pieces.
                UInt64 victimBitboard = bitboard[(1 - colour)] ^ enemyPawnBitboard;
                value += PawnAttackValue * Bit.CountSparse(_pawnAttackBitboard[colour] & victimBitboard);

                // Evaluate pawn defence to friendly minor pieces. 
                UInt64 lowValueBitboard = bitboard[colour | Piece.Bishop] | bitboard[colour | Piece.Knight] | bitboard[colour | Piece.Pawn];
                value += PawnDefenceValue * Bit.Count(_pawnAttackBitboard[colour] & lowValueBitboard);

                if (colour == position.SideToMove)
                    totalValue += value;
                else
                    totalValue -= value;
            }

            // Evaluate asymetric features (immediate captures). 
            {
                Int32 colour = position.SideToMove;

                // Pawn takes queen.
                if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Queen]) != 0)
                    totalValue += PieceValue[Piece.Queen] - PieceValue[Piece.Pawn];

                // Minor takes queen. 
                else if ((_minorAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Queen]) != 0)
                    totalValue += PieceValue[Piece.Queen] - PieceValue[Piece.Bishop];

                // Pawn takes rook. 
                else if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Rook]) != 0)
                    totalValue += PieceValue[Piece.Rook] - PieceValue[Piece.Pawn];

                // Pawn takes bishop. 
                else if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Bishop]) != 0)
                    totalValue += PieceValue[Piece.Bishop] - PieceValue[Piece.Pawn];

                // Pawn takes knight. 
                else if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Knight]) != 0)
                    totalValue += PieceValue[Piece.Knight] - PieceValue[Piece.Pawn];

                // Minor takes rook. 
                else if ((_minorAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Rook]) != 0)
                    totalValue += PieceValue[Piece.Rook] - PieceValue[Piece.Bishop];
            }
            
            return (Int32)totalValue;
        }

        /// <summary>
        /// Returns the estimated material exchange value of the given move on the 
        /// given position as determined by static analysis.
        /// </summary>
        /// <param name="position">The position the move is to be played on.</param>
        /// <param name="move">The move to evaluate.</param>
        /// <returns>The estimated material exchange value of the move.</returns>
        private static Int32 EvaluateStaticExchange(Position position, Int32 move) {
            Int32 from = Move.From(move);
            Int32 to = Move.To(move);
            Int32 piece = Move.Piece(move);
            Int32 capture = Move.Capture(move);

            position.Bitboard[piece] ^= 1UL << from;
            position.OccupiedBitboard ^= 1UL << from;
            position.Square[to] = piece;

            Int32 value = 0;
            if (Move.IsPromotion(move)) {
                Int32 promotion = Move.Special(move);
                position.Square[to] = promotion;
                value += PieceValue[promotion] - PieceValue[Piece.Pawn];
            }
            value += PieceValue[capture] - EvaluateStaticExchange(position, 1 - position.SideToMove, to);

            position.Bitboard[piece] ^= 1UL << from;
            position.OccupiedBitboard ^= 1UL << from;
            position.Square[to] = capture;

            return value;
        }

        /// <summary>
        /// Returns the estimated material exchange value of moving a piece to the 
        /// given square and performing captures on the square as necessary as 
        /// determined by static analysis. 
        /// </summary>
        /// <param name="position">The position the square is to be moved to.</param>
        /// <param name="colour">The side to move.</param>
        /// <param name="square">The square to move to.</param>
        /// <returns>The estimated material exchange value of moving to the square.</returns>
        private static Int32 EvaluateStaticExchange(Position position, Int32 colour, Int32 square) {
            Int32 value = 0;
            Int32 from = SmallestAttackerSquare(position, colour, square);
            if (from != Position.InvalidSquare) {
                Int32 piece = position.Square[from];
                Int32 capture = position.Square[square];

                position.Bitboard[piece] ^= 1UL << from;
                position.OccupiedBitboard ^= 1UL << from;
                position.Square[square] = piece;

                value = Math.Max(0, PieceValue[capture] - EvaluateStaticExchange(position, 1 - colour, square));

                position.Bitboard[piece] ^= 1UL << from;
                position.OccupiedBitboard ^= 1UL << from;
                position.Square[square] = capture;
            }
            return value;
        }

        /// <summary>
        /// Returns the square of the piece with the lowest material value that can 
        /// move to the given square. 
        /// </summary>
        /// <param name="position">The position to find the square for.</param>
        /// <param name="colour">The side to find the square for.</param>
        /// <param name="square">The square to move to.</param>
        /// <returns>The square of the piece with the lowest material value that can move to the given square.</returns>
        private static Int32 SmallestAttackerSquare(Position position, Int32 colour, Int32 square) {

            // Try pawns.
            UInt64 sourceBitboard = position.Bitboard[colour | Piece.Pawn] & Attack.Pawn(square, 1 - colour);
            if (sourceBitboard != 0)
                return Bit.Scan(sourceBitboard);

            // Try knights. 
            sourceBitboard = position.Bitboard[colour | Piece.Knight] & Attack.Knight(square);
            if (sourceBitboard != 0)
                return Bit.Scan(sourceBitboard);

            // Try bishops. 
            UInt64 bishopAttackBitboard = UInt64.MaxValue;

            if ((position.Bitboard[colour | Piece.Bishop] & Bit.Diagonals[square]) != 0) {
                bishopAttackBitboard = Attack.Bishop(square, position.OccupiedBitboard);
                sourceBitboard = position.Bitboard[colour | Piece.Bishop] & bishopAttackBitboard;
                if (sourceBitboard != 0)
                    return Bit.Scan(sourceBitboard);
            }

            // Try rooks. 
            UInt64 rookAttackBitboard = UInt64.MaxValue;

            if ((position.Bitboard[colour | Piece.Rook] & Bit.Axes[square]) != 0) {
                rookAttackBitboard = Attack.Rook(square, position.OccupiedBitboard);
                sourceBitboard = position.Bitboard[colour | Piece.Rook] & rookAttackBitboard;
                if (sourceBitboard != 0)
                    return Bit.Scan(sourceBitboard);
            }

            // Try queens. 
            if ((position.Bitboard[colour | Piece.Queen] & (Bit.Diagonals[square] | Bit.Axes[square])) != 0) {
                if (bishopAttackBitboard == UInt64.MaxValue)
                    bishopAttackBitboard = Attack.Bishop(square, position.OccupiedBitboard);
                if (rookAttackBitboard == UInt64.MaxValue)
                    rookAttackBitboard = Attack.Rook(square, position.OccupiedBitboard);

                sourceBitboard = position.Bitboard[colour | Piece.Queen] & (bishopAttackBitboard | rookAttackBitboard);
                if (sourceBitboard != 0)
                    return Bit.Scan(sourceBitboard);
            }

            // Try king. 
            sourceBitboard = position.Bitboard[colour | Piece.King] & Attack.King(square);
            if (sourceBitboard != 0)
                return Bit.Read(sourceBitboard);

            return Position.InvalidSquare;
        }

    }
}
