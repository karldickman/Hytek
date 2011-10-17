using System;
using System.Collections.Generic;
using System.Linq;
using Ngol.Hytek.Interfaces;
using Ngol.Utilities.TextFormat;

namespace Ngol.Hytek
{
    /// <summary>
    /// A formatter for races.
    /// </summary>
    public class RaceFormatter : IFormatter<IRace>
    {
        #region Properties

        /// <summary>
        /// The formatter for the results.
        /// </summary>
        ResultsFormatter ResultsFormatter
        {
            get;
            set;
        }

        /// <summary>
        /// The formatter for the scores.
        /// </summary>
        ScoreFormatter ScoreFormatter
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new formatter.
        /// </summary>
        public RaceFormatter()
        {
            ResultsFormatter = new ResultsFormatter();
            ScoreFormatter = new ScoreFormatter();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format a race the way Hytek Meet Manger does.
        /// </summary>
        /// <param name="race">
        /// A <see cref="IRace"/> to be formatted.
        /// </param>
        /// <param name="showHeader">
        /// Should a header describing race location be shown or not?
        /// </param>
        public IEnumerable<string> Format(IRace race, bool showHeader=false)
        {
            // Convert to list to ensure that enumeration happens only once
            IEnumerable<string> resultsLines = ResultsFormatter.Format(race.Gender, race.Distance, race.Results).ToList();
            int width = resultsLines.Max(line => line.Length) + 2;
            // Blank line before anything else
            yield return string.Empty;
            if(showHeader)
            {
                yield return StringFormatting.Centered(race.Meet.Name, width);
                yield return StringFormatting.Centered(race.Date.ToString(), width);
                yield return StringFormatting.Centered(race.Meet.Venue, width);
                yield return string.Empty;
            }
            foreach(string line in resultsLines)
            {
                yield return string.Format(" {0} ", line);
            }
            // Blank line between results and scores.
            yield return string.Empty;
            foreach(string line in ScoreFormatter.Format(race.Scores.OrderBy(score => score)))
            {
                yield return string.Format(" {0} ", line);
            }
            // Blank line after anything else
            yield return string.Empty;
        }

        IEnumerable<string> IFormatter<IRace>.Format(IRace race)
        {
            return Format(race);
        }

        #endregion
    }
}
