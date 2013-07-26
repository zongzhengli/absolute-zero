using System;

namespace AbsoluteZero {

    /// <summary>
    /// Provides PRNG methods. 
    /// </summary>
    static class Random {
        private static UInt64 seed = (UInt64)DateTime.Now.ToFileTime();

        static Random() {
            for (Int32 i = 0; i < 10; i++)
                UInt64();
        }

        public static UInt64 UInt64() {
            seed ^= seed << 13;
            seed ^= seed >> 7;
            seed ^= seed << 17;
            return seed;
        }

        public static Double Double() {
            return UInt64() * 5.4210108624275222e-20;
        }

        public static Double Double(Double a, Double b = 0) {
            return a + Double() * (b - a);
        }

        public static Int32 Int32(Int32 a, Int32 b = 0) {
            Double correction = .5 * Math.Sign((Double)a - b);
            return (Int32)Math.Round(Double(a + correction, b - correction));
        }

        public static Boolean Boolean() {
            return (UInt64() & 1) == 0;
        }
    }
}

