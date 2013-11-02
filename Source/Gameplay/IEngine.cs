using System;
using System.Collections.Generic;

namespace AbsoluteZero {

    /// <summary>
    /// Defines a chess engine.
    /// </summary>
    interface IEngine : IPlayer {
        List<Int32> PrincipalVariation { get; }

        Int64 Nodes { get; }

        Int32 HashAllocation { get; set; }
    }
}
