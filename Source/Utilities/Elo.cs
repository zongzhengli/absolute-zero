using System;

namespace AbsoluteZero {
    static class Elo {
        public const Int32 MaximumMagnitude = 999;
        private const Double Sigma = 1.96;

        public static Double GetDifference(Int32 wins, Int32 losses, Int32 draws) {
            return GetDifference((wins + .5 * draws) / (wins + losses + draws));
        }

        public static Double GetDifference(Double score) {
            Double value = -400 * Math.Log10(1 / score - 1);
            if (Math.Abs(value) > MaximumMagnitude)
                return Math.Sign(value) * MaximumMagnitude;
            return value;
        }

        public static Double GetError(Int32 wins, Int32 losses, Int32 draws) {
            Double totalGames = wins + losses + draws;
            Double score = (wins + .5 * draws) / totalGames;
            Double drawProportion = draws / totalGames;
            Double margin = Sigma * Math.Sqrt((score * (1 - score) - .25 * drawProportion) / totalGames);
            if (margin <= 0)
                return GetError(wins + 1, losses + 1, draws);
            return GetDifference(.5 + margin);
        }
    }
}
