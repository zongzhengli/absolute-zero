using System;

namespace AbsoluteZero {
    public enum MatchResult { Win, Loss, Draw, Unresolved }
    public enum MatchOptions { None, RandomizeColour, UnlimitedLength }

    static class Match {
        public static Int32 HalfMovesLimit = 200;

        public static MatchResult Play(IEngine white, IEngine black, Position position, MatchOptions option = MatchOptions.None) {
            if (option == MatchOptions.RandomizeColour) {
                MatchResult result;
                if (Random.Boolean()) {
                    result = Play(black, white, position);
                    if (result == MatchResult.Win)
                        result = MatchResult.Loss;
                    else if (result == MatchResult.Loss)
                        result = MatchResult.Win;
                } else
                    result = Play(white, black, position);
                return result;
            }

            Int32 halfMovesLimit = HalfMovesLimit;
            if (option == MatchOptions.UnlimitedLength)
                halfMovesLimit = Int32.MaxValue;

            while (true) {
                IPlayer player = position.SideToMove == Piece.White ? white : black;
                position.Make(player.GetMove(position));

                if (position.LegalMoves().Count == 0)
                    if (position.InCheck(position.SideToMove))
                        return player.Equals(white) ? MatchResult.Win : MatchResult.Loss;
                    else
                        return MatchResult.Draw;
                if (position.FiftyMovesClock >= 100 || position.InsufficientMaterial() || position.HasRepeated(3))
                    return MatchResult.Draw;
                if (position.HalfMoves >= halfMovesLimit)
                    return MatchResult.Unresolved;
            }
        }
    }
}
