using System;

namespace AbsoluteZero {

    /// <summary>
    /// Specifies the match result. 
    /// </summary>
    public enum MatchResult { Win, Loss, Draw, Unresolved }

    /// <summary>
    /// Specifies the properties of the match. 
    /// </summary>
    public enum MatchOptions { None, RandomizeColour, UnlimitedLength }

    /// <summary>
    /// Provides methods for playing matches between two engine. 
    /// </summary>
    static class Match {

        /// <summary>
        /// The number of plies before the match is terminated as unresolved. 
        /// </summary>
        public static Int32 HalfMovesLimit = 300;

        /// <summary>
        /// The material difference at which an unresolved match is given to the side 
        /// with more material. 
        /// </summary>
        public static Int32 MaterialLimit = Zero.PieceValue[Piece.Bishop] + 1;

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
                if (Random.Boolean()) {
                    MatchResult result = Play(black, white, position, MatchOptions.None);
                    return result == MatchResult.Win ? MatchResult.Loss :
                           result == MatchResult.Loss ? MatchResult.Win :
                                                        result;
                } else
                    return Play(white, black, position, MatchOptions.None);
            }

            Int32 halfMovesLimit = HalfMovesLimit;
            if (option == MatchOptions.UnlimitedLength)
                halfMovesLimit = Int32.MaxValue;

            // Play the match. 
            while (true) {
                IPlayer player = position.SideToMove == Colour.White ? white : black;
                position.Make(player.GetMove(position));

                if (position.LegalMoves().Count == 0)
                    if (position.InCheck(position.SideToMove))
                        return player.Equals(white) ? MatchResult.Win : MatchResult.Loss;
                    else
                        return MatchResult.Draw;

                if (position.FiftyMovesClock >= 100 || position.InsufficientMaterial() || position.HasRepeated(3))
                    return MatchResult.Draw;

                if (position.HalfMoves >= halfMovesLimit) {
                    int materialDifference = position.Material[Colour.White] + position.Material[Colour.Black];
                    if (Math.Abs(materialDifference) >= MaterialLimit)
                        return materialDifference > 0 ? MatchResult.Win : MatchResult.Loss;
                    return MatchResult.Unresolved;
                }
            }
        }
    }
}
