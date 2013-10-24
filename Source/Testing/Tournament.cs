using System;
using System.Collections.Generic;
using System.IO;

namespace AbsoluteZero {

    /// <summary>
    /// Facilitates a tournament between two engine instances. 
    /// </summary>
    static class Tournament {

        /// <summary>
        /// The string that determines output formatting. 
        /// </summary>
        private static readonly String ResultFormat = String.Format("     {{0,-{0}}}{{1,-{0}}}{{2,-{0}}}{{3,-{0}}}{{4,-{0}}}{{5}}", ColumnWidth);

        /// <summary>
        /// The number of matches between file updates. 
        /// </summary>
        private const Int32 UpdateInterval = 10;

        /// <summary>
        /// The number of characters in a column for output. 
        /// </summary>
        private const Int32 ColumnWidth = 8;

        /// <summary>
        /// The unique ID code for the tournament.  
        /// </summary>
        private static String ID = "Tournament " + DateTime.Now.ToString().Replace('/', '-').Replace(':', '.');

        /// <summary>
        /// Begins the tournament with the given parameters. 
        /// </summary>
        /// <param name="parameters">Command-line parameters giving the conditions of the tournament.</param>
        public static void Run(String[] parameters) {
            IEngine experimental = new Zero() {
                IsExperimental = true
            };
            IEngine standard = new Zero();

            List<String> epd = TestSuite.Parse(parameters);
            Restrictions.Output = OutputType.None;
            Int32 wins = 0;
            Int32 losses = 0;
            Int32 draws = 0;

            using (StreamWriter sw = new StreamWriter(ID + ".txt")) {
                sw.WriteLine(new String(' ', UpdateInterval) + String.Format(ResultFormat, "Games", "Wins", "Losses", "Draws", "Elo", "Error"));
                sw.WriteLine("-----------------------------------------------------------------");

                // Play the tournament. 
                for (Int32 games = 1; ; games++) {
                    sw.Flush();
                    experimental.Reset();
                    standard.Reset();
                    Position position = new Position(epd[Random.Int32(epd.Count - 1)]);
                    MatchResult result = Match.Play(experimental, standard, position, MatchOptions.RandomizeColour);

                    // Write the match result. 
                    switch (result) {
                        case MatchResult.Win:
                            sw.Write('1');
                            wins++;
                            break;
                        case MatchResult.Loss:
                            sw.Write('0');
                            losses++;
                            break;
                        case MatchResult.Draw:
                            sw.Write('-');
                            draws++;
                            break;
                        case MatchResult.Unresolved:
                            sw.Write('*');
                            draws++;
                            break;
                    }

                    // Write the cummulative results. 
                    if (games % UpdateInterval == 0) {
                        String elo = Format.PrecisionAndSign(Elo.GetDifference(wins, losses, draws));
                        String error = "±" + Math.Round(Elo.GetError(wins, losses, draws));

                        sw.WriteLine(String.Format(ResultFormat, games, wins, losses, draws, elo, error));
                    }
                }
            }
        }
    }
}
