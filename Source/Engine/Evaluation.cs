using System;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the evaluation component of the Absolute Zero chess engine. 
    /// </summary>
    partial class Zero {

        /// <summary>
        /// Returns the estimated value of the given position as determined by static 
        /// analysis. 
        /// </summary>
        /// <param name="position">The position to evaluate.</param>
        /// <returns>The estimated value of the position.</returns>
        private Int32 Evaluate(Position position) {
            UInt64[] bitboard = position.Bitboard;
            Int32 colour;
            Int32 sign;

            Single value = position.Material[Piece.White] + position.Material[Piece.Black];
            Single opening = PhaseCoefficient * Math.Min(position.Material[Piece.White], -position.Material[Piece.Black]);
            Single endgame = 1 - opening;

            _pawnAttackBitboard[Piece.White] = (bitboard[Piece.White | Piece.Pawn] & NotAFileBitboard) >> 9;
            _pawnAttackBitboard[Piece.White] |= (bitboard[Piece.White | Piece.Pawn] & NotHFileBitboard) >> 7;
            _pawnAttackBitboard[Piece.Black] = (bitboard[Piece.Black | Piece.Pawn] & NotAFileBitboard) << 7;
            _pawnAttackBitboard[Piece.Black] |= (bitboard[Piece.Black | Piece.Pawn] & NotHFileBitboard) << 9;
            _kingSquare[Piece.White] = Bit.Read(bitboard[Piece.White | Piece.King]);
            _kingSquare[Piece.Black] = Bit.Read(bitboard[Piece.Black | Piece.King]);

            for (colour = Piece.White; colour <= Piece.Black; colour++) {
                UInt64 targetBitboard = ~bitboard[colour] & ~_pawnAttackBitboard[1 - colour];
                UInt64 pawnBitboard = bitboard[colour | Piece.Pawn];
                UInt64 enemyBitboard = bitboard[(1 - colour) | Piece.Pawn];
                UInt64 allPawnBitboard = pawnBitboard | enemyBitboard;
                sign = -2 * colour + 1;

                // Evaluate king. 
                Int32 square = _kingSquare[colour];
                value += opening * KingOpeningPositionValue[colour][square] + endgame * KingEndgamePositionValue[colour][square];
                value += opening * PawnNearKingValue * Bit.Count(PawnShieldBitboard[square] & pawnBitboard) * sign;
                
                if ((allPawnBitboard & Bit.File[square]) == 0)
                    value += opening * KingOnOpenFileValue * sign;

                if (Position.File(square) > 0 && (allPawnBitboard & Bit.File[square - 1]) == 0)
                    value += opening * KingAdjacentToOpenFileValue * sign;

                if (Position.File(square) < 7 && (allPawnBitboard & Bit.File[square + 1]) == 0)
                    value += opening * KingAdjacentToOpenFileValue * sign;

                // Evaluate bishops. 
                UInt64 pieceBitboard = bitboard[colour | Piece.Bishop];
                _minorAttackBitboard[colour] = 0;

                if ((pieceBitboard & (pieceBitboard - 1)) != 0)
                    value += BishopPairValue * sign;

                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += BishopPositionValue[colour][square];

                    UInt64 pseudoMoveBitboard = Attack.Bishop(square, position.OccupiedBitboard);
                    value += BishopMobilityValue[Bit.Count(targetBitboard & pseudoMoveBitboard)] * sign;
                    _minorAttackBitboard[colour] |= pseudoMoveBitboard;
                }

                // Evaluate knights. 
                pieceBitboard = bitboard[colour | Piece.Knight];
                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += opening * KnightOpeningPositionValue[colour][square];
                    value += endgame * KnightToEnemyKingSpatialValue[square][_kingSquare[1 - colour]] * sign;

                    UInt64 pseudoMoveBitboard = Attack.Knight(square);
                    value += KnightMobilityValue[Bit.Count(targetBitboard & pseudoMoveBitboard)] * sign;
                    _minorAttackBitboard[colour] |= pseudoMoveBitboard;
                }

                // Evaluate queens. 
                pieceBitboard = bitboard[colour | Piece.Queen];
                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += opening * QueenOpeningPositionValue[colour][square];
                    value += endgame * QueenToEnemyKingSpatialValue[square][_kingSquare[1 - colour]] * sign;
                }

                // Evaluate rooks. 
                pieceBitboard = bitboard[colour | Piece.Rook];
                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += RookPositionValue[colour][square];
                }

                // Evaluate pawns.
                Int32 pawns = 0;
                pieceBitboard = bitboard[colour | Piece.Pawn];
                while (pieceBitboard != 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += PawnPositionValue[colour][square];
                    pawns++;

                    if ((ShortForwardFileBitboard[colour][square] & pawnBitboard) != 0)
                        value += DoubledPawnValue * sign;

                    else if ((PawnBlockadeBitboard[colour][square] & enemyBitboard) == 0)
                        value += (PassedPawnValue + endgame * PassedPawnEndgamePositionValue[colour][square]) * sign;

                    if ((ShortAdjacentFilesBitboard[square] & pawnBitboard) == 0)
                        value += IsolatedPawnValue * sign;
                }
                value += (pawns == 0 ? PawnDeficiencyValue : pawns * endgame * PawnEndgameGainValue) * sign;

                // Evaluate pawn threat to enemy minor pieces.
                UInt64 victimBitboard = bitboard[(1 - colour)] ^ enemyBitboard;
                value += PawnAttackValue * Bit.CountSparse(_pawnAttackBitboard[colour] & victimBitboard) * sign;

                // Evaluate pawn defence to friendly minor pieces. 
                UInt64 lowValueBitboard = bitboard[colour | Piece.Bishop] | bitboard[colour | Piece.Knight] | bitboard[colour | Piece.Pawn];
                value += PawnDefenceValue * Bit.Count(_pawnAttackBitboard[colour] & lowValueBitboard) * sign;
            }

            // Evaluate obvious captures. 
            colour = position.SideToMove;
            sign = -2 * colour + 1;

            // Pawn takes queen.
            if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Queen]) != 0)
                value += (PieceValue[Piece.Queen] - PieceValue[Piece.Pawn]) * sign;

            // Minor takes queen. 
            else if ((_minorAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Queen]) != 0)
                value += (PieceValue[Piece.Queen] - PieceValue[Piece.Bishop]) * sign;

            // Pawn takes rook. 
            else if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Rook]) != 0)
                value += (PieceValue[Piece.Rook] - PieceValue[Piece.Pawn]) * sign;

            // Pawn takes bishop. 
            else if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Bishop]) != 0)
                value += (PieceValue[Piece.Bishop] - PieceValue[Piece.Pawn]) * sign;

            // Pawn takes knight. 
            else if ((_pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Knight]) != 0)
                value += (PieceValue[Piece.Knight] - PieceValue[Piece.Pawn]) * sign;

            // Minor takes rook. 
            else if ((_minorAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Rook]) != 0)
                value += (PieceValue[Piece.Rook] - PieceValue[Piece.Bishop]) * sign;

            return (Int32)value * sign + TempoValue;
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
                value += PieceValue[promotion] - PieceValue[position.SideToMove | Piece.Pawn];
            }
            value += EvaluateStaticExchange(position, 1 - position.SideToMove, to) - PieceValue[capture];

            position.Bitboard[piece] ^= 1UL << from;
            position.OccupiedBitboard ^= 1UL << from;
            position.Square[to] = capture;

            return value * (-2 * position.SideToMove + 1);
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

                value = EvaluateStaticExchange(position, 1 - colour, square) - PieceValue[capture];
                value = colour == Piece.White ? Math.Max(0, value) : Math.Min(0, value);

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
