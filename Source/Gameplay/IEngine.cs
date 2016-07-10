using System;
using System.Collections.Generic;

namespace AbsoluteZero {

    /// <summary>
    /// Defines a chess engine.
    /// </summary>
    interface IEngine : IPlayer {

        /// <summary>
        /// Returns the principal variation of the most recent search.
        /// </summary>
        /// <returns>The principal variation of the most recent search.</returns>
        List<Int32> GetPrincipalVariation();

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
