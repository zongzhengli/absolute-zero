using System;

namespace AbsoluteZero {

    /// <summary>
    /// Provides methods for attack bitboard generation. 
    /// </summary>
    static class Attack {

        /// <summary>
        /// The collection of king attack bitboards. KingAttack[s] gives a bitboard 
        /// of the squares attacked by a king on square s. 
        /// </summary>
        private static UInt64[] KingAttack = new UInt64[64];

        /// <summary>
        /// The collection of knight attack bitboards. KnightAttack[s] gives a 
        /// bitboard of the squares attacked by a knight on square s. 
        /// </summary>
        private static UInt64[] KnightAttack = new UInt64[64];

        /// <summary>
        /// The collection of pawn attack bitboards. PawnAttack[c][s] gives a 
        /// bitboard of the squares attacked by a pawn of colour c on square s. 
        /// </summary>
        private static UInt64[][] PawnAttack = { new UInt64[64], new UInt64[64] };

        /// <summary>
        /// The collection of cached queen attack bitboards. _cachedQueenAttack[s] 
        /// gives the last generated queen attack bitboard for square s. 
        /// </summary>
        private static UInt64[] _cachedQueenAttack = new UInt64[64];

        /// <summary>
        /// The collection of cached queen block bitboards. _cachedQueenBlock[s] 
        /// gives the bitboard of the squares where a queen's attack from square s 
        /// was last blocked. 
        /// </summary>
        private static UInt64[] _cachedQueenBlock = new UInt64[64];

        /// <summary>
        /// The collection of cached rook attack bitboards. _cachedRookAttack[s] 
        /// gives the last generated rook attack bitboard for square s. 
        /// </summary>
        private static UInt64[] _cachedRookAttack = new UInt64[64];

        /// <summary>
        /// The collection of cached rook block bitboards. _cachedRookBlock[s] gives 
        /// the bitboard of the squares where a rook's attack from square s was last 
        /// blocked. 
        /// </summary>
        private static UInt64[] _cachedRookBlock = new UInt64[64];

        /// <summary>
        /// The collection of cached bishop attack bitboards. _cachedBishopAttack[s] 
        /// gives the last generated bishop attack bitboard for square s. 
        /// </summary>
        private static UInt64[] _cachedBishopAttack = new UInt64[64];

        /// <summary>
        /// The collection of cached bishop block bitboards. _cachedBishopBlock[s] 
        /// gives the bitboard of the squares where a bishop's attack from square s 
        /// was last blocked. 
        /// </summary>
        private static UInt64[] _cachedBishopBlock = new UInt64[64];

        /// <summary>
        /// Initializes lookup tables. 
        /// </summary>
        static Attack() {
            for (Int32 square = 0; square < 64; square++) {

                // Initialize cached block bitboards. 
                _cachedQueenBlock[square] = UInt64.MaxValue;
                _cachedRookBlock[square] = UInt64.MaxValue;
                _cachedBishopBlock[square] = UInt64.MaxValue;

                Int32 file = Position.File(square);
                Int32 rank = Position.Rank(square);

                // Initialize king attack bitboards. 
                for (Int32 a = -1; a <= 1; a++)
                    for (Int32 b = -1; b <= 1; b++)
                        if (a != 0 || b != 0)
                            KingAttack[square] ^= TryGetBitboard(file + a, rank + b);

                // Initialize knight attack bitboards. 
                for (Int32 a = -2; a <= 2; a++)
                    for (Int32 b = -2; b <= 2; b++)
                        if (Math.Abs(a) + Math.Abs(b) == 3)
                            KnightAttack[square] ^= TryGetBitboard(file + a, rank + b);

                // Initialize pawn attack bitboards. 
                PawnAttack[Colour.White][square] ^= TryGetBitboard(file - 1, rank - 1);
                PawnAttack[Colour.White][square] ^= TryGetBitboard(file + 1, rank - 1);
                PawnAttack[Colour.Black][square] ^= TryGetBitboard(file - 1, rank + 1);
                PawnAttack[Colour.Black][square] ^= TryGetBitboard(file + 1, rank + 1);
            }
        }

        /// <summary>
        /// Returns a bitboard consisting of a single filled square with the given 
        /// file and rank. If an invalid square is specified the empty bitboard is 
        /// returned. 
        /// </summary>
        /// <param name="file">The file of the square.</param>
        /// <param name="rank">The rank of the square.</param>
        /// <returns>A bitboard consisting of a single filled square.</returns>
        public static UInt64 TryGetBitboard(Int32 file, Int32 rank) {
            if (file < 0 || file >= 8 || rank < 0 || rank >= 8)
                return 0;
            return 1UL << (file + rank * 8);
        }

        /// <summary>
        /// Returns the king's attack bitboard for the given square. 
        /// </summary>
        /// <param name="square">The square the king is on.</param>
        /// <returns>The king's attack bitboard.</returns>
        public static UInt64 King(Int32 square) {
            return KingAttack[square];
        }

        /// <summary>
        /// Returns the queen's attack bitboard for the given square with the given 
        /// occupancy bitboard. 
        /// </summary>
        /// <param name="square">The square the queen is on.</param>
        /// <param name="occupiedBitboard">The occupancy bitboard.</param>
        /// <returns>The queen's attack bitboard.</returns>
        public static UInt64 Queen(Int32 square, UInt64 occupiedBitboard) {
            if ((_cachedQueenAttack[square] & occupiedBitboard) != _cachedQueenBlock[square]) {
                _cachedQueenAttack[square] = Rook(square, occupiedBitboard) | Bishop(square, occupiedBitboard);
                _cachedQueenBlock[square] = _cachedQueenAttack[square] & occupiedBitboard;
            }
            return _cachedQueenAttack[square];
        }

        /// <summary>
        /// Returns the rook's attack bitboard for the given square with the given 
        /// occupancy bitboard.
        /// </summary>
        /// <param name="square">The square the rook is on.</param>
        /// <param name="occupiedBitboard">The occupancy bitboard.</param>
        /// <returns>The rook's attack bitboard.</returns>
        public static UInt64 Rook(Int32 square, UInt64 occupiedBitboard) {
            if ((_cachedRookAttack[square] & occupiedBitboard) != _cachedRookBlock[square]) {
                UInt64 attackBitboard = Bit.RayN[square];
                UInt64 blockBitboard = attackBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    attackBitboard ^= Bit.RayN[Bit.ScanReverse(blockBitboard)];

                UInt64 partialBitboard = Bit.RayE[square];
                blockBitboard = partialBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    partialBitboard ^= Bit.RayE[Bit.Scan(blockBitboard)];
                attackBitboard |= partialBitboard;

                partialBitboard = Bit.RayS[square];
                blockBitboard = partialBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    partialBitboard ^= Bit.RayS[Bit.Scan(blockBitboard)];
                attackBitboard |= partialBitboard;

                partialBitboard = Bit.RayW[square];
                blockBitboard = partialBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    partialBitboard ^= Bit.RayW[Bit.ScanReverse(blockBitboard)];
                attackBitboard |= partialBitboard;

                _cachedRookAttack[square] = attackBitboard;
                _cachedRookBlock[square] = attackBitboard & occupiedBitboard;
            }
            return _cachedRookAttack[square];
        }

        /// <summary>
        /// Returns the bishop's attack bitboard for the given square with the given 
        /// occupancy bitboard.
        /// </summary>
        /// <param name="square">The square the bishop is on.</param>
        /// <param name="occupiedBitboard">The occupancy bitboard.</param>
        /// <returns>The bishop's attack bitboard.</returns>
        public static UInt64 Bishop(Int32 square, UInt64 occupiedBitboard) {
            if ((_cachedBishopAttack[square] & occupiedBitboard) != _cachedBishopBlock[square]) {
                UInt64 attackBitboard = Bit.RayNE[square];
                UInt64 blockBitboard = attackBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    attackBitboard ^= Bit.RayNE[Bit.ScanReverse(blockBitboard)];

                UInt64 partialBitboard = Bit.RayNW[square];
                blockBitboard = partialBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    partialBitboard ^= Bit.RayNW[Bit.ScanReverse(blockBitboard)];
                attackBitboard |= partialBitboard;

                partialBitboard = Bit.RaySE[square];
                blockBitboard = partialBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    partialBitboard ^= Bit.RaySE[Bit.Scan(blockBitboard)];
                attackBitboard |= partialBitboard;

                partialBitboard = Bit.RaySW[square];
                blockBitboard = partialBitboard & occupiedBitboard;
                if (blockBitboard != 0)
                    partialBitboard ^= Bit.RaySW[Bit.Scan(blockBitboard)];
                attackBitboard |= partialBitboard;

                _cachedBishopAttack[square] = attackBitboard;
                _cachedBishopBlock[square] = attackBitboard & occupiedBitboard;
            }
            return _cachedBishopAttack[square];
        }

        /// <summary>
        /// Returns the knight's attack bitboard for the given square. 
        /// </summary>
        /// <param name="square">The square the knight is on.</param>
        /// <returns>The knight's attack bitboard.</returns>
        public static UInt64 Knight(Int32 square) {
            return KnightAttack[square];
        }

        /// <summary>
        /// Returns the pawn's attack bitboard for the given square as the given 
        /// colour. 
        /// </summary>
        /// <param name="square">The square the pawn is on.</param>
        /// <param name="colour">The colour of the pawn.</param>
        /// <returns>The pawn's attack bitboard.</returns>
        public static UInt64 Pawn(Int32 square, Int32 colour) {
            return PawnAttack[colour][square];
        }

        /// <summary>
        /// Returns a bitboard that gives the result of performing a floodfill by 
        /// traversing via knight moves. 
        /// </summary>
        /// <param name="square">The square to start the fill at.</param>
        /// <param name="moves">The number of moves for the fill.</param>
        /// <returns>A bitboard that is the result of the knight floodfill.</returns>
        public static UInt64 KnightFill(Int32 square, Int32 moves) {
            if (moves <= 0)
                return 0;
            UInt64 bitboard = Knight(square);
            UInt64 copy = bitboard;
            while (copy != 0)
                bitboard |= KnightFill(Bit.Pop(ref copy), moves - 1);
            return bitboard;
        }
    }
}
