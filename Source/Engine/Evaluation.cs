using System;

namespace AbsoluteZero {

    /// <summary>
    /// The evaluation component of the Absolute Zero chess engine. 
    /// </summary>
    partial class Zero {

        private Int32 Evaluate(Position position) {
            UInt64[] bitboard = position.Bitboard;
            Single value = position.Material[Piece.White] + position.Material[Piece.Black];
            Single opening = PhaseCoefficient * Math.Min(position.Material[Piece.White], -position.Material[Piece.Black]);
            Single endgame = 1 - opening;

            pawnAttackBitboard[Piece.White] = (bitboard[Piece.White | Piece.Pawn] & NotAFileBitboard) >> 9;
            pawnAttackBitboard[Piece.White] |= (bitboard[Piece.White | Piece.Pawn] & NotHFileBitboard) >> 7;
            pawnAttackBitboard[Piece.Black] = (bitboard[Piece.Black | Piece.Pawn] & NotAFileBitboard) << 7;
            pawnAttackBitboard[Piece.Black] |= (bitboard[Piece.Black | Piece.Pawn] & NotHFileBitboard) << 9;
            kingSquare[Piece.White] = Bit.Read(bitboard[Piece.White | Piece.King]);
            kingSquare[Piece.Black] = Bit.Read(bitboard[Piece.Black | Piece.King]);

            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++) {
                Int32 sign = -2 * colour + 1;
                UInt64 targetBitboard = ~bitboard[colour | Piece.All] & ~pawnAttackBitboard[1 - colour];
                UInt64 pawnBitboard = bitboard[colour | Piece.Pawn];
                UInt64 enemyBitboard = bitboard[(1 - colour) | Piece.Pawn];
                UInt64 allPawnBitboard = pawnBitboard | enemyBitboard;

                // king evaluation
                Int32 square = kingSquare[colour];
                value += opening * KingOpeningPositionValue[colour][square] + endgame * KingEndgamePositionValue[colour][square];
                value += opening * PawnNearKingValue * Bit.Count(PawnShieldBitboard[square] & pawnBitboard) * sign;
                if ((allPawnBitboard & Bit.File[square]) <= 0)
                    value += opening * KingOnOpenFileValue * sign;
                if (Position.File(square) > 0 && (allPawnBitboard & Bit.File[square - 1]) <= 0)
                    value += opening * KingAdjacentToOpenFileValue * sign;
                if (Position.File(square) < 7 && (allPawnBitboard & Bit.File[square + 1]) <= 0)
                    value += opening * KingAdjacentToOpenFileValue * sign;

                // bishop evaluation
                UInt64 pieceBitboard = bitboard[colour | Piece.Bishop];
                if ((pieceBitboard & (pieceBitboard - 1)) > 0)
                    value += BishopPairValue * sign;
                minorAttackBitboard[colour] = 0;
                while (pieceBitboard > 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += BishopPositionValue[colour][square];
                    UInt64 pseudoMoveBitboard = Attack.Bishop(square, position.OccupiedBitboard);
                    minorAttackBitboard[colour] |= pseudoMoveBitboard;
                    value += BishopMobilityValue[Bit.Count(targetBitboard & pseudoMoveBitboard)] * sign;
                }

                // knight evaluation
                pieceBitboard = bitboard[colour | Piece.Knight];
                while (pieceBitboard > 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += opening * KnightOpeningPositionValue[colour][square];
                    value += endgame * KnightToEnemyKingSpatialValue[square][kingSquare[1 - colour]] * sign;
                    UInt64 pseudoMoveBitboard = Attack.Knight(square);
                    minorAttackBitboard[colour] |= pseudoMoveBitboard;
                    value += KnightMobilityValue[Bit.Count(targetBitboard & pseudoMoveBitboard)] * sign;
                }

                // queen evaluation
                pieceBitboard = bitboard[colour | Piece.Queen];
                while (pieceBitboard > 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += opening * QueenOpeningPositionValue[colour][square];
                    value += endgame * QueenToEnemyKingSpatialValue[square][kingSquare[1 - colour]] * sign;
                }

                // rook evaluation
                pieceBitboard = bitboard[colour | Piece.Rook];
                while (pieceBitboard > 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += RookPositionValue[colour][square];
                }

                // pawn evaluation
                Int32 pawns = 0;
                pieceBitboard = bitboard[colour | Piece.Pawn];
                while (pieceBitboard > 0) {
                    square = Bit.Pop(ref pieceBitboard);
                    value += PawnPositionValue[colour][square];
                    if ((ShortForwardFileBitboard[colour][square] & pawnBitboard) > 0)
                        value += DoubledPawnValue * sign;
                    else if ((PawnBlockadeBitboard[colour][square] & enemyBitboard) <= 0)
                        value += (PassedPawnValue + endgame * PassedPawnEndgamePositionValue[colour][square]) * sign;
                    if ((ShortAdjacentFilesBitboard[square] & pawnBitboard) <= 0)
                        value += IsolatedPawnValue * sign;
                    pawns++;
                }
                if (pawns > 0)
                    value += pawns * endgame * PawnEndgameGainValue * sign;
                else
                    value += PawnDeficiencyValue * sign;

                // pawn threat evaluation
                UInt64 victimBitboard = bitboard[(1 - colour) | Piece.All] ^ enemyBitboard;
                value += PawnAttackValue * Bit.CountSparse(pawnAttackBitboard[colour] & victimBitboard) * sign;

                // pawn defence evaluation
                UInt64 lowValueBitboard = bitboard[colour | Piece.Bishop] | bitboard[colour | Piece.Knight] | bitboard[colour | Piece.Pawn];
                value += PawnDefenceValue * Bit.Count(pawnAttackBitboard[colour] & lowValueBitboard) * sign;
            }

            // capture evaluation
            {
                Int32 colour = position.SideToMove;
                Int32 sign = -2 * colour + 1;

                // pawn takes queen
                if ((pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Queen]) > 0)
                    value += (PieceValue[Piece.Queen] - PieceValue[Piece.Pawn]) * sign;

                // minor takes queen
                else if ((minorAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Queen]) > 0)
                    value += (PieceValue[Piece.Queen] - PieceValue[Piece.Bishop]) * sign;

                // pawn takes rook
                else if ((pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Rook]) > 0)
                    value += (PieceValue[Piece.Rook] - PieceValue[Piece.Pawn]) * sign;

                // pawn takes bishop
                else if ((pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Bishop]) > 0)
                    value += (PieceValue[Piece.Bishop] - PieceValue[Piece.Pawn]) * sign;

                // pawn takes knight
                else if ((pawnAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Knight]) > 0)
                    value += (PieceValue[Piece.Knight] - PieceValue[Piece.Pawn]) * sign;

                // minor takes rook
                else if ((minorAttackBitboard[colour] & bitboard[(1 - colour) | Piece.Rook]) > 0)
                    value += (PieceValue[Piece.Rook] - PieceValue[Piece.Bishop]) * sign;
            }
            return (Int32)value * (-2 * position.SideToMove + 1) + TempoValue;
        }

        private static Int32 EvaluateStaticExchange(Position position, Int32 move) {
            Int32 from = Move.GetFrom(move);
            Int32 to = Move.GetTo(move);
            Int32 piece = Move.GetPiece(move);
            Int32 capture = Move.GetCapture(move);

            position.Bitboard[piece] ^= 1UL << from;
            position.OccupiedBitboard ^= 1UL << from;
            position.Square[to] = piece;

            Int32 value = 0;
            if (Move.IsPromotion(move)) {
                Int32 promotion = Move.GetSpecial(move);
                position.Square[to] = promotion;
                value += PieceValue[promotion] - PieceValue[Piece.Pawn];
            }
            value += EvaluateStaticExchange(position, 1 - position.SideToMove, to) - PieceValue[capture];

            position.Bitboard[piece] ^= 1UL << from;
            position.OccupiedBitboard ^= 1UL << from;
            position.Square[to] = capture;

            return value * (-2 * position.SideToMove + 1);
        }

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

        public static Int32 SmallestAttackerSquare(Position position, Int32 colour, Int32 square) {
            UInt64 sourceBitboard = position.Bitboard[colour | Piece.Pawn] & Attack.Pawn(square, 1 - colour);
            if (sourceBitboard > 0)
                return Bit.Scan(sourceBitboard);

            sourceBitboard = position.Bitboard[colour | Piece.Knight] & Attack.Knight(square);
            if (sourceBitboard > 0)
                return Bit.Scan(sourceBitboard);

            UInt64 bishopAttackBitboard = UInt64.MaxValue;
            if ((position.Bitboard[colour | Piece.Bishop] & Attack.Diagonals[square]) > 0) {
                bishopAttackBitboard = Attack.Bishop(square, position.OccupiedBitboard);
                sourceBitboard = position.Bitboard[colour | Piece.Bishop] & bishopAttackBitboard;
                if (sourceBitboard > 0)
                    return Bit.Scan(sourceBitboard);
            }

            UInt64 rookAttackBitboard = UInt64.MaxValue;
            if ((position.Bitboard[colour | Piece.Rook] & Attack.Axes[square]) > 0) {
                rookAttackBitboard = Attack.Rook(square, position.OccupiedBitboard);
                sourceBitboard = position.Bitboard[colour | Piece.Rook] & rookAttackBitboard;
                if (sourceBitboard > 0)
                    return Bit.Scan(sourceBitboard);
            }

            if ((position.Bitboard[colour | Piece.Queen] & (Attack.Diagonals[square] | Attack.Axes[square])) > 0) {
                if (bishopAttackBitboard == UInt64.MaxValue)
                    bishopAttackBitboard = Attack.Bishop(square, position.OccupiedBitboard);
                if (rookAttackBitboard == UInt64.MaxValue)
                    rookAttackBitboard = Attack.Rook(square, position.OccupiedBitboard);
                sourceBitboard = position.Bitboard[colour | Piece.Queen] & (bishopAttackBitboard | rookAttackBitboard);
                if (sourceBitboard > 0)
                    return Bit.Scan(sourceBitboard);
            }

            sourceBitboard = position.Bitboard[colour | Piece.King] & Attack.King(square);
            if (sourceBitboard > 0)
                return Bit.Read(sourceBitboard);
            return Position.InvalidSquare;
        }

    }
}
