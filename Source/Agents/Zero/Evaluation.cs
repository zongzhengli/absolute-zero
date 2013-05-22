using System;

namespace AbsoluteZero {
    partial class Zero {
        private Int32 Evaluate(Position position) {
            UInt64[] bitField = position.BitField;
            Single value = position.Material[Piece.White] + position.Material[Piece.Black];
            Single opening = PhaseCoefficient * Math.Min(position.Material[Piece.White], -position.Material[Piece.Black]);
            Single endgame = 1 - opening;

            pawnAttackField[Piece.White] = (bitField[Piece.White | Piece.Pawn] & NotAFileField) >> 9;
            pawnAttackField[Piece.White] |= (bitField[Piece.White | Piece.Pawn] & NotHFileField) >> 7;
            pawnAttackField[Piece.Black] = (bitField[Piece.Black | Piece.Pawn] & NotAFileField) << 7;
            pawnAttackField[Piece.Black] |= (bitField[Piece.Black | Piece.Pawn] & NotHFileField) << 9;
            kingSquare[Piece.White] = Bit.Read(bitField[Piece.White | Piece.King]);
            kingSquare[Piece.Black] = Bit.Read(bitField[Piece.Black | Piece.King]);

            for (Int32 colour = Piece.White; colour <= Piece.Black; colour++) {
                Int32 sign = -2 * colour + 1;
                UInt64 targetField = ~bitField[colour | Piece.All] & ~pawnAttackField[1 - colour];
                UInt64 pawnField = bitField[colour | Piece.Pawn];
                UInt64 enemyPawnField = bitField[(1 - colour) | Piece.Pawn];
                UInt64 allPawnField = pawnField | enemyPawnField;

                // king evaluation
                Int32 square = kingSquare[colour];
                value += opening * KingOpeningPositionValue[colour][square] + endgame * KingEndgamePositionValue[colour][square];
                value += opening * PawnNearKingValue * Bit.Count(PawnShieldField[square] & pawnField) * sign;
                if ((allPawnField & Bit.File[square]) <= 0)
                    value += opening * KingOnOpenFileValue * sign;
                if (Position.File(square) > 0 && (allPawnField & Bit.File[square - 1]) <= 0)
                    value += opening * KingAdjacentToOpenFileValue * sign;
                if (Position.File(square) < 7 && (allPawnField & Bit.File[square + 1]) <= 0)
                    value += opening * KingAdjacentToOpenFileValue * sign;

                // bishop evaluation
                UInt64 pieceField = bitField[colour | Piece.Bishop];
                if ((pieceField & (pieceField - 1)) > 0)
                    value += BishopPairValue * sign;
                minorAttackField[colour] = 0;
                while (pieceField > 0) {
                    square = Bit.Pop(ref pieceField);
                    value += BishopPositionValue[colour][square];
                    UInt64 pseudoMoveField = Attack.Bishop(square, position.OccupiedField);
                    minorAttackField[colour] |= pseudoMoveField;
                    value += BishopMobilityValue[Bit.Count(targetField & pseudoMoveField)] * sign;
                }

                // knight evaluation
                pieceField = bitField[colour | Piece.Knight];
                while (pieceField > 0) {
                    square = Bit.Pop(ref pieceField);
                    value += opening * KnightOpeningPositionValue[colour][square];
                    value += endgame * KnightToEnemyKingSpatialValue[square][kingSquare[1 - colour]] * sign;
                    UInt64 pseudoMoveField = Attack.Knight(square);
                    minorAttackField[colour] |= pseudoMoveField;
                    value += KnightMobilityValue[Bit.Count(targetField & pseudoMoveField)] * sign;
                }

                // queen evaluation
                pieceField = bitField[colour | Piece.Queen];
                while (pieceField > 0) {
                    square = Bit.Pop(ref pieceField);
                    value += opening * QueenOpeningPositionValue[colour][square];
                    value += endgame * QueenToEnemyKingSpatialValue[square][kingSquare[1 - colour]] * sign;
                }

                // rook evaluation
                pieceField = bitField[colour | Piece.Rook];
                while (pieceField > 0) {
                    square = Bit.Pop(ref pieceField);
                    value += RookPositionValue[colour][square];
                }

                // pawn evaluation
                Int32 pawns = 0;
                pieceField = bitField[colour | Piece.Pawn];
                while (pieceField > 0) {
                    square = Bit.Pop(ref pieceField);
                    value += PawnPositionValue[colour][square];
                    if ((ShortForwardFileField[colour][square] & pawnField) > 0)
                        value += DoubledPawnValue * sign;
                    else if ((PawnBlockadeField[colour][square] & enemyPawnField) <= 0)
                        value += (PassedPawnValue + endgame * PassedPawnEndgamePositionValue[colour][square]) * sign;
                    if ((ShortAdjacentFilesField[square] & pawnField) <= 0)
                        value += IsolatedPawnValue * sign;
                    pawns++;
                }
                if (pawns > 0)
                    value += pawns * endgame * PawnEndgameGainValue * sign;
                else
                    value += PawnDeficiencyValue * sign;

                // pawn threat evaluation
                UInt64 victimField = bitField[(1 - colour) | Piece.All] ^ enemyPawnField;
                value += PawnAttackValue * Bit.CountSparse(pawnAttackField[colour] & victimField) * sign;

                // pawn defence evaluation
                UInt64 lowValueField = bitField[colour | Piece.Bishop] | bitField[colour | Piece.Knight] | bitField[colour | Piece.Pawn];
                value += PawnDefenceValue * Bit.Count(pawnAttackField[colour] & lowValueField) * sign;
            }

            // capture evaluation
            {
                Int32 colour = position.Colour;
                Int32 sign = -2 * colour + 1;

                // pawn takes queen
                if ((pawnAttackField[colour] & bitField[(1 - colour) | Piece.Queen]) > 0)
                    value += (PieceValue[Piece.Queen] - PieceValue[Piece.Pawn]) * sign;

                // minor takes queen
                else if ((minorAttackField[colour] & bitField[(1 - colour) | Piece.Queen]) > 0)
                    value += (PieceValue[Piece.Queen] - PieceValue[Piece.Bishop]) * sign;

                // pawn takes rook
                else if ((pawnAttackField[colour] & bitField[(1 - colour) | Piece.Rook]) > 0)
                    value += (PieceValue[Piece.Rook] - PieceValue[Piece.Pawn]) * sign;

                // pawn takes bishop
                else if ((pawnAttackField[colour] & bitField[(1 - colour) | Piece.Bishop]) > 0)
                    value += (PieceValue[Piece.Bishop] - PieceValue[Piece.Pawn]) * sign;

                // pawn takes knight
                else if ((pawnAttackField[colour] & bitField[(1 - colour) | Piece.Knight]) > 0)
                    value += (PieceValue[Piece.Knight] - PieceValue[Piece.Pawn]) * sign;

                // minor takes rook
                else if ((minorAttackField[colour] & bitField[(1 - colour) | Piece.Rook]) > 0)
                    value += (PieceValue[Piece.Rook] - PieceValue[Piece.Bishop]) * sign;
            }
            return (Int32)value * (-2 * position.Colour + 1) + TempoValue;
        }

        private static Int32 EvaluateStaticExchange(Position position, Int32 move) {
            Int32 from = Move.GetFrom(move);
            Int32 to = Move.GetTo(move);
            Int32 piece = Move.GetPiece(move);
            Int32 capture = Move.GetCapture(move);

            position.BitField[piece] ^= 1UL << from;
            position.OccupiedField ^= 1UL << from;
            position.Element[to] = piece;

            Int32 value = 0;
            if (Move.IsPromotion(move)) {
                Int32 promotion = Move.GetSpecial(move);
                position.Element[to] = promotion;
                value += PieceValue[promotion] - PieceValue[Piece.Pawn];
            }
            value += EvaluateStaticExchange(position, 1 - position.Colour, to) - PieceValue[capture];

            position.BitField[piece] ^= 1UL << from;
            position.OccupiedField ^= 1UL << from;
            position.Element[to] = capture;

            return value * (-2 * position.Colour + 1);
        }

        private static Int32 EvaluateStaticExchange(Position position, Int32 colour, Int32 square) {
            Int32 value = 0;
            Int32 from = SmallestAttackerSquare(position, colour, square);
            if (from != Position.InvalidSquare) {
                Int32 piece = position.Element[from];
                Int32 capture = position.Element[square];

                position.BitField[piece] ^= 1UL << from;
                position.OccupiedField ^= 1UL << from;
                position.Element[square] = piece;

                value = EvaluateStaticExchange(position, 1 - colour, square) - PieceValue[capture];
                value = colour == Piece.White ? Math.Max(0, value) : Math.Min(0, value);

                position.BitField[piece] ^= 1UL << from;
                position.OccupiedField ^= 1UL << from;
                position.Element[square] = capture;
            }
            return value;
        }

        public static Int32 SmallestAttackerSquare(Position position, Int32 colour, Int32 square) {
            UInt64 sourceField = position.BitField[colour | Piece.Pawn] & Attack.Pawn(square, 1 - colour);
            if (sourceField > 0)
                return Bit.Scan(sourceField);

            sourceField = position.BitField[colour | Piece.Knight] & Attack.Knight(square);
            if (sourceField > 0)
                return Bit.Scan(sourceField);

            UInt64 bishopAttackField = UInt64.MaxValue;
            if ((position.BitField[colour | Piece.Bishop] & Attack.Diagonals[square]) > 0) {
                bishopAttackField = Attack.Bishop(square, position.OccupiedField);
                sourceField = position.BitField[colour | Piece.Bishop] & bishopAttackField;
                if (sourceField > 0)
                    return Bit.Scan(sourceField);
            }

            UInt64 rookAttackField = UInt64.MaxValue;
            if ((position.BitField[colour | Piece.Rook] & Attack.Axes[square]) > 0) {
                rookAttackField = Attack.Rook(square, position.OccupiedField);
                sourceField = position.BitField[colour | Piece.Rook] & rookAttackField;
                if (sourceField > 0)
                    return Bit.Scan(sourceField);
            }

            if ((position.BitField[colour | Piece.Queen] & (Attack.Diagonals[square] | Attack.Axes[square])) > 0) {
                if (bishopAttackField == UInt64.MaxValue)
                    bishopAttackField = Attack.Bishop(square, position.OccupiedField);
                if (rookAttackField == UInt64.MaxValue)
                    rookAttackField = Attack.Rook(square, position.OccupiedField);
                sourceField = position.BitField[colour | Piece.Queen] & (bishopAttackField | rookAttackField);
                if (sourceField > 0)
                    return Bit.Scan(sourceField);
            }

            sourceField = position.BitField[colour | Piece.King] & Attack.King(square);
            if (sourceField > 0)
                return Bit.Read(sourceField);
            return Position.InvalidSquare;
        }

    }
}
