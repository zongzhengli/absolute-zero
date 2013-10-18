using System;

namespace AbsoluteZero {
    interface IPlayer {
        String Name { get; }

        Boolean AcceptDraw { get; }

        Int32 GetMove(Position position);

        void Stop();

        void Reset();
    }
}
