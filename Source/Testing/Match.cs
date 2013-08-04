using System;

namespace AbsoluteZero {

    /// <summary>
    /// Specifies the match result. 
    /// </summary>
    public enum MatchResult { Win, Loss, Draw, Unresolved }

    /// <summary>
    /// Specifies the match option. 
    /// </summary>
    public enum MatchOptions { None, RandomizeColour, UnlimitedLength }

    /// <summary>
    /// Facilitates a match between two engine instances. 
    /// </summary>
    static class Match {

        /// <summary>
        /// The number of plies in a match before it can be terminated as a draw. 
        /// </summary>
        public static Int32 HalfMovesLimit = 200;

        /// <summary>
        /// Facilitates play between the two engines for the given position with the 
        /// given match option. 
        /// </summary>
        /// <param name="white">The engine instance playing as white.</param>
        /// <param name="black">The engine instance playing as black.</param>
        /// <param name="position">The position the match is played on.</param>
        /// <param name="option">The match option specifying the conditions of the match.</param>
        /// <returns>The result of the match from white's perspective.</returns>
        public static MatchResult Play(IEngine white, IEngine black, Position position, MatchOptions option = MatchOptions.None) {

            // If randomize colour is given as the match option, give a 50% chance of 
            // of swapping white and black. The result is still returned from the 
            // original white's perspective. 
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

            // Play the match. 
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
