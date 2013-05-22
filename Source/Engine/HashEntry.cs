using System;

namespace AbsoluteZero {
   partial class Zero {
        private struct HashEntry {
            public const Int32 Invalid = 0;
            public const Int32 Exact = 1;
            public const Int32 Alpha = 2;
            public const Int32 Beta = 3;

            private const Int32 TypeBits = 2;
            private const Int32 DepthBits = 8;

            private const Int32 DepthShift = TypeBits;
            private const Int32 ValueShift = DepthShift + DepthBits;
            private const Int32 DepthNormal = 1 << (DepthBits - 1);
            private const Int32 ValueNormal = Int16.MaxValue;

            private const Int32 TypeMask = (1 << TypeBits) - 1;
            private const Int32 DepthMask = (1 << DepthBits) - 1;

            public const Int32 Size = 16;

            public UInt64 Key;
            public Int32 Move;
            public Int32 Misc;

            public HashEntry(Position position, Int32 depth, Int32 ply, Int32 move, Int32 value, Int32 type) {
                Key = position.ZobristKey;
                Move = move;
                if (Math.Abs(value) > NearCheckmateValue)
                    value += Math.Sign(value) * ply;
                Misc = type | ((depth + DepthNormal) << DepthShift) | (value + ValueNormal) << ValueShift;
            }

            public new Int32 GetType() {
                return Misc & TypeMask;
            }

            public Int32 GetDepth() {
                return ((Misc >> DepthShift) - DepthNormal) & DepthMask;
            }

            public Int32 GetValue(Int32 ply) {
                Int32 value = (Misc >> ValueShift) - ValueNormal;
                if (Math.Abs(value) > NearCheckmateValue)
                    return value - Math.Sign(value) * ply;
                return value;
            }
        }
    }
}
