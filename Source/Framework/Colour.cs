using System;

namespace AbsoluteZero {

    /// <summary>
    /// Defines colours and masks. 
    /// </summary>
    static class Colour {

        /// <summary>
        /// The mask for extracting the colour of a piece. 
        /// </summary>
        public const Int32 Mask = Black;                                             // 0001

        /// <summary>
        /// The colour white. 
        /// </summary>
        public const Int32 White = 0;                                                // 0000

        /// <summary>
        /// The colour black. 
        /// </summary>
        public const Int32 Black = 1;                                                // 0001
    }
}
