using System;
using System.Collections.Generic;

namespace AbsoluteZero {
    interface IEngine : IPlayer {
        List<Int32> GetPV();

        Int64 GetNodes();

        void AllocateHash(Int32 megabytes);
    }
}
