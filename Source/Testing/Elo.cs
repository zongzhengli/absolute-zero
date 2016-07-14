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
        /// The standard deviation for a two-sided 90% confidence interval. 
        /// </summary>
        public const Double Z90 = 1.6448602374021966;

        /// <summary>
        /// The standard deviation for a two-sided 95% confidence interval. 
        /// </summary>
        public const Double Z95 = 1.959955286319985;

        /// <summary>
        /// The standard deviation for a two-sided 99% confidence interval. 
        /// </summary>
        public const Double Z99 = 2.5758042845158604;
        
        /// <summary>
        /// Returns the difference in Elo between two players given wins, losses, and 
        /// draws between matches they've played. 
        /// </summary>
        /// <param name="wins">The number of wins for the calculating player.</param>
        /// <param name="losses">The number of losses for the calculating player.</param>
        /// <param name="draws">The number of draws.</param>
        /// <returns>The difference in Elo from the perspective of the calculating player.</returns>
        public static Double GetDelta(Int32 wins, Int32 losses, Int32 draws) {
            return GetDelta((wins + 0.5 * draws) / (wins + losses + draws));
        }

        /// <summary>
        /// Returns the difference in Elo between two players given one of their 
        /// scores where score = (wins + draws / 2) / n and n is the number of 
        /// matches played. 
        /// </summary>
        /// <param name="score">The score of the calculating player.</param>
        /// <returns>The difference in Elo from the perspective of the calculating player.</returns>
        public static Double GetDelta(Double score) {
            return -400 * Math.Log10(1 / score - 1);
        }

        /// <summary>
        /// Returns the Elo difference error margin for the given results at the 
        /// given level of significance. This uses the Wald interval from the normal 
        /// approximation of the trinomial distribution and behaves poorly for small 
        /// or extreme samples.
        /// </summary>
        /// <param name="wins">The number of wins for the player.</param>
        /// <param name="losses">The number of losses for the player.</param>
        /// <param name="draws">The number of draws.</param>
        /// <returns>An array where the first element is the lower margin and second element is the upper margin.</returns>
            Double n = wins + losses + draws;
            Double p = (wins + 0.5 * draws) / n;
            Double sd = Math.Sqrt((wins * Math.Pow(1 - p, 2) + losses * Math.Pow(0 - p, 2) + draws * Math.Pow(0.5 - p, 2)) / (n - 1));
            Double se = sd / Math.Sqrt(n);
            Double elo = GetDelta(p);
            return new Double[] { lower - elo, upper - elo };
        }

        /// <summary>
        /// Returns whether the result from GetError() is likely trustworthy as
        /// determined by a rule of thumb.
        /// </summary>
        /// <param name="sigma">The standard score.</param>
        /// <param name="wins">The number of wins for the player.</param>
        /// <param name="losses">The number of losses for the player.</param>
        /// <param name="draws">The number of draws.</param>
        /// <returns>Whether the error bound for the Elo difference calculation holds.</returns>
        public static Boolean IsErrorValid(Int32 wins, Int32 losses, Int32 draws) {
            Double n = wins + losses + draws;
            Double p = (wins + 0.5 * draws) / n;
            return n * p > 5 && n * (1 - p) > 5;
        }
    }
}
