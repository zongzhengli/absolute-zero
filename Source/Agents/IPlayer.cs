using System;

namespace AbsoluteZero {
    interface IPlayer {
        Int32 GetMove(Position position);

        String GetName();

        Boolean AcceptDraw();

        void Stop();

        void Reset();
    }
}
