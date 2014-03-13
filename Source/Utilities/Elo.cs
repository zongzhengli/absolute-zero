using System;

namespace AbsoluteZero {

    /// <summary>
    /// Provides methods for Elo calculation.
    /// </summary>
    static class Elo {

        /// <summary>
        /// The maximum magnitude for results. 
        /// </summary>
        private const Int32 MaxValue = 999;

        /// <summary>
        /// The standard deviation for the error bound calculations. 
        /// </summary>
        private const Double Sigma = 1.96;
        
        /// <summary>
        /// Returns the difference in Elo between two players given wins, losses, and 
        /// draws between matches they've played. 
        /// </summary>
        /// <param name="wins">The number of wins for the calculating player.</param>
        /// <param name="losses">The number of losses for the calculating player.</param>
        /// <param name="draws">The number of draws.</param>
        /// <returns>The difference in Elo from the perspective of the calculating player.</returns>
        public static Double GetDifference(Int32 wins, Int32 losses, Int32 draws) {
            return GetDifference((wins + .5 * draws) / (wins + losses + draws));
        }

        /// <summary>
        /// Returns the difference in Elo between two players given one of their 
        /// scores where score = (wins + draws / 2) / n and n is the number of 
        /// matches played. 
        /// </summary>
        /// <param name="score">The score of the calculating player.</param>
        /// <returns>The difference in Elo from the perspective of the calculating player.</returns>
        public static Double GetDifference(Double score) {
            Double value = -400 * Math.Log10(1 / score - 1);
            if (Math.Abs(value) > MaxValue)
                return Math.Sign(value) * MaxValue;
            return value;
        }

        /// <summary>
        /// Returns the error bound for the Elo difference calculation between two 
        /// players given the wins, losses, and draws between matches they've 
        /// played. 
        /// </summary>
        /// <param name="wins">The number of wins for the calculating player.</param>
        /// <param name="losses">The number of losses for the calculating player.</param>
        /// <param name="draws">The number of draws.</param>
        /// <returns>The error bound for the Elo difference calculation.</returns>
        public static Double GetError(Int32 wins, Int32 losses, Int32 draws) {
            Double totalGames = wins + losses + draws;
            Double score = (wins + .5 * draws) / totalGames;
            Double drawProportion = draws / totalGames;
            Double margin = Sigma * Math.Sqrt((score * (1 - score) - .25 * drawProportion) / totalGames);
            if (margin == 0)
                return GetError(wins + 1, losses + 1, draws);
            return GetDifference(.5 + margin);
        }
    }
}
