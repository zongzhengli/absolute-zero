using System;

namespace AbsoluteZero {

    /// <summary>
    /// Specifies the output type.
    /// </summary>
    public enum OutputType { Standard, Universal, None }

    /// <summary>
    /// Defines restrictions for engines in standard games. 
    /// </summary>
    public static class Restrictions {

        /// <summary>
        /// The output type.
        /// </summary>
        public static OutputType Output = OutputType.Standard;

        /// <summary>
        /// The maximum number of milliseconds to use when moving. 
        /// </summary>
        public static Int32 MoveTime;

        /// <summary>
        /// The maximum depth to search to when moving.
        /// </summary>
        public static Int32 Depth;

        /// <summary>
        /// The maximum number of nodes to search when moving. 
        /// </summary>
        public static Int64 Nodes;

        /// <summary>
        /// The minimum number of principal variations to search when moving. 
        /// </summary>
        public static Int32 PrincipalVariations;

        /// <summary>
        /// Whether to use time controls. 
        /// </summary>
        public static Boolean UseTimeControls;

        /// <summary>
        /// The time left for given side. TimeControl[c] gives the number of 
        /// milliseconds left on the clock for colour c.
        /// </summary>
        public static Int32[] TimeControl;

        /// <summary>
        /// The time increment for given side. TimeIncrement[c] gives the number of 
        /// milliseconds incremented for colour c after every move.
        /// </summary>
        public static Int32[] TimeIncrement;

        /// <summary>
        /// Initializes default values. 
        /// </summary>
        static Restrictions() {
            Reset();
        }

        /// <summary>
        /// Resets the restrictions to the default values, with the exception of 
        /// output type.
        /// </summary>
        public static void Reset() {
            UseTimeControls = false;
            TimeControl = new Int32[2];
            TimeIncrement = new Int32[2];
            MoveTime = Int32.MaxValue;
            Depth = Zero.DepthLimit;
            Nodes = Int64.MaxValue;
            PrincipalVariations = 1;
        }
    }
}
