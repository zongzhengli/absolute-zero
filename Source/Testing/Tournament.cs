using System;
using System.Collections.Generic;
using System.IO;

namespace AbsoluteZero {

    /// <summary>
    /// Provides methods for playing a tournament between two engines. 
    /// </summary>
    public static class Tournament {

        /// <summary>
        /// Specifies header and result formatting. 
        /// </summary>
        private static readonly String ResultFormat = "     {0,-8}{1,-8}{2,-8}{3,-8}{4, -8}{5}";

        /// <summary>
        /// The number of matches between file updates. 
        /// </summary>
        private const Int32 UpdateInterval = 10;

        /// <summary>
        /// The unique ID code for the tournament.  
        /// </summary>
        private static String ID = "Tournament " + DateTime.Now.ToString().Replace('/', '-').Replace(':', '.');

        /// <summary>
        /// Begins the tournament with the given positions. 
        /// </summary>
        /// <param name="epd">A list of positions to play in EPD format.</param>
        public static void Run(List<String> epd) {
            Engine experimental = new Engine() { IsExperimental = true };
            Engine standard = new Engine();

            Restrictions.Output = OutputType.None;
            Int32 wins = 0;
            Int32 losses = 0;
            Int32 draws = 0;

            using (StreamWriter sw = new StreamWriter(ID + ".txt")) {
                sw.WriteLine(new String(' ', UpdateInterval) + String.Format(ResultFormat, "Games", "Wins", "Losses", "Draws", "Elo", "Error"));
                sw.WriteLine("--------------------------------------------------------------------");

                // Play the tournament. 
                for (Int32 games = 1; ; games++) {
                    sw.Flush();
                    experimental.Reset();
                    standard.Reset();
                    Position position = Position.Create(epd[Random.Int32(epd.Count - 1)]);
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
                        Double delta = Elo.GetDelta(wins, losses, draws);
                        String elo = String.Format("{0:+0;-0}", delta);

                        Double[] bound = Elo.GetError(Elo.Z95, wins, losses, draws);
                        Double lower = Math.Max(bound[0], -999);
                        Double upper = Math.Min(bound[1], 999);
                        String asterisk = Elo.IsErrorValid(wins, losses, draws) ? String.Empty : "*";
                        String error = String.Format("{0:+0;-0} {1:+0;-0}{2}", lower, upper, asterisk);

                        sw.WriteLine(String.Format(ResultFormat, games, wins, losses, draws, elo, error));
                    }
                }
            }
        }
    }
}
