using System;
using System.Collections.Generic;

namespace AbsoluteZero {

    /// <summary>
    /// Defines a chess engine.
    /// </summary>
    interface IEngine : IPlayer {

        /// <summary>
        /// The principal variation of the most recent search. 
        /// </summary>
        List<Int32> PrincipalVariation { get; }

        /// <summary>
        /// The number of nodes visited during the most recent search. 
        /// </summary>
        Int64 Nodes { get; }

        /// <summary>
        /// The size of the transposition table in megabytes. 
        /// </summary>
        Int32 HashAllocation { get; set; }
    }
}
