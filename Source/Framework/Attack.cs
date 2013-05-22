using System;

namespace AbsoluteZero {
    static class Attack {
        public static UInt64[] RayN = new UInt64[64];
        public static UInt64[] RayE = new UInt64[64];
        public static UInt64[] RayS = new UInt64[64];
        public static UInt64[] RayW = new UInt64[64];
        public static UInt64[] RayNE = new UInt64[64];
        public static UInt64[] RayNW = new UInt64[64];
        public static UInt64[] RaySE = new UInt64[64];
        public static UInt64[] RaySW = new UInt64[64];
        public static UInt64[] Axes = new UInt64[64];
        public static UInt64[] Diagonals = new UInt64[64];
        private static UInt64[] KingAttack = new UInt64[64];
        private static UInt64[] KnightAttack = new UInt64[64];
        private static UInt64[][] PawnAttack = { new UInt64[64], new UInt64[64] };

        private static UInt64[] Relevant = new UInt64[64];
        private static UInt64[] cachedQueenAttack = new UInt64[64];
        private static UInt64[] cachedQueenBlock = new UInt64[64];
        private static UInt64[] cachedRookAttack = new UInt64[64];
        private static UInt64[] cachedRookBlock = new UInt64[64];
        private static UInt64[] cachedBishopAttack = new UInt64[64];
        private static UInt64[] cachedBishopBlock = new UInt64[64];

        static Attack() {
            for (Int32 square = 0; square < 64; square++) {
                RayN[square] = Bit.DirectionFill(square, 0, -1) ^ (1UL << square);
                RayE[square] = Bit.DirectionFill(square, 1, 0) ^ (1UL << square);
                RayS[square] = Bit.DirectionFill(square, 0, 1) ^ (1UL << square);
                RayW[square] = Bit.DirectionFill(square, -1, 0) ^ (1UL << square);
                RayNE[square] = Bit.DirectionFill(square, 1, -1) ^ (1UL << square);
                RayNW[square] = Bit.DirectionFill(square, -1, -1) ^ (1UL << square);
                RaySE[square] = Bit.DirectionFill(square, 1, 1) ^ (1UL << square);
                RaySW[square] = Bit.DirectionFill(square, -1, 1) ^ (1UL << square);
                Axes[square] = RayN[square] | RayE[square] | RayS[square] | RayW[square];
                Diagonals[square] = RayNE[square] | RayNW[square] | RaySE[square] | RaySW[square];

                cachedQueenBlock[square] = UInt64.MaxValue;
                cachedRookBlock[square] = UInt64.MaxValue;
                cachedBishopBlock[square] = UInt64.MaxValue;

                Int32 file = Position.File(square);
                Int32 rank = Position.Rank(square);

                // king
                for (Int32 a = -1; a <= 1; a++)
                    for (Int32 b = -1; b <= 1; b++)
                        if (a != 0 || b != 0)
                            KingAttack[square] ^= TryGetField(file + a, rank + b);

                // knight
                for (Int32 a = -2; a <= 2; a++)
                    for (Int32 b = -2; b <= 2; b++)
                        if (Math.Abs(a) + Math.Abs(b) == 3)
                            KnightAttack[square] ^= TryGetField(file + a, rank + b);

                // pawn
                PawnAttack[Piece.White][square] ^= TryGetField(file - 1, rank - 1);
                PawnAttack[Piece.White][square] ^= TryGetField(file + 1, rank - 1);
                PawnAttack[Piece.Black][square] ^= TryGetField(file - 1, rank + 1);
                PawnAttack[Piece.Black][square] ^= TryGetField(file + 1, rank + 1);
            }
        }

        public static UInt64 TryGetField(Int32 file, Int32 rank) {
            if (file < 0 || file >= 8 || rank < 0 || rank >= 8)
                return 0;
            return 1UL << (file + rank * 8);
        }

        public static UInt64 King(Int32 square) {
            return KingAttack[square];
        }

        public static UInt64 Queen(Int32 square, UInt64 occupiedField) {
            if ((cachedQueenAttack[square] & occupiedField) != cachedQueenBlock[square]) {
                cachedQueenAttack[square] = Rook(square, occupiedField) | Bishop(square, occupiedField);
                cachedQueenBlock[square] = cachedQueenAttack[square] & occupiedField;
            }
            return cachedQueenAttack[square];
        }

        public static UInt64 Rook(Int32 square, UInt64 occupiedField) {
            if ((cachedRookAttack[square] & occupiedField) != cachedRookBlock[square]) {
                UInt64 attackField = RayN[square];
                UInt64 blockField = attackField & occupiedField;
                if (blockField > 0)
                    attackField ^= RayN[Bit.ScanReverse(blockField)];

                UInt64 partialField = RayE[square];
                blockField = partialField & occupiedField;
                if (blockField > 0)
                    partialField ^= RayE[Bit.Scan(blockField)];
                attackField |= partialField;

                partialField = RayS[square];
                blockField = partialField & occupiedField;
                if (blockField > 0)
                    partialField ^= RayS[Bit.Scan(blockField)];
                attackField |= partialField;

                partialField = RayW[square];
                blockField = partialField & occupiedField;
                if (blockField > 0)
                    partialField ^= RayW[Bit.ScanReverse(blockField)];
                attackField |= partialField;

                cachedRookAttack[square] = attackField;
                cachedRookBlock[square] = attackField & occupiedField;
            }
            return cachedRookAttack[square];
        }

        public static UInt64 Bishop(Int32 square, UInt64 occupiedField) {
            if ((cachedBishopAttack[square] & occupiedField) != cachedBishopBlock[square]) {
                UInt64 attackField = RayNE[square];
                UInt64 blockField = attackField & occupiedField;
                if (blockField > 0)
                    attackField ^= RayNE[Bit.ScanReverse(blockField)];

                UInt64 partialField = RayNW[square];
                blockField = partialField & occupiedField;
                if (blockField > 0)
                    partialField ^= RayNW[Bit.ScanReverse(blockField)];
                attackField |= partialField;

                partialField = RaySE[square];
                blockField = partialField & occupiedField;
                if (blockField > 0)
                    partialField ^= RaySE[Bit.Scan(blockField)];
                attackField |= partialField;

                partialField = RaySW[square];
                blockField = partialField & occupiedField;
                if (blockField > 0)
                    partialField ^= RaySW[Bit.Scan(blockField)];
                attackField |= partialField;

                cachedBishopAttack[square] = attackField;
                cachedBishopBlock[square] = attackField & occupiedField;
            }
            return cachedBishopAttack[square];
        }

        public static UInt64 Knight(Int32 square) {
            return KnightAttack[square];
        }

        public static UInt64 Pawn(Int32 square, Int32 colour) {
            return PawnAttack[colour][square];
        }

        public static UInt64 KnightFill(Int32 square, Int32 moves) {
            if (moves <= 0)
                return 0;
            UInt64 field = Knight(square);
            UInt64 copy = field;
            while (copy > 0)
                field |= KnightFill(Bit.Pop(ref copy), moves - 1);
            return field;
        }
    }
}
