using System;

namespace AbsoluteZero {
    public enum OutputType { Standard, Universal, None }

    static class Restrictions {
        public static OutputType Output = OutputType.Standard;
        public static Int32 MoveTime;
        public static Int32 Depth;
        public static Int64 Nodes;

        public static Boolean UseTimeControls;
        public static Int32[] TimeControl;
        public static Int32[] TimeIncrement;

        static Restrictions() {
            Reset();
        }

        public static void Reset() {
            UseTimeControls = false;
            TimeControl = new Int32[2];
            TimeIncrement = new Int32[2];
            MoveTime = Int32.MaxValue;
            Depth = Zero.DepthLimit;
            Nodes = 1000000000000;
        }
    }
}
