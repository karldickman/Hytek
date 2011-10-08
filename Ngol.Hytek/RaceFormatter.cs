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
        public IEnumerable<string> Format(IRace race)
        {
            return Format(race, false);
        }

        /// <summary>
        /// Format a race the way Hytek Meet Manger does.
        /// </summary>
        /// <param name="race">
        /// A <see cref="IRace"/> to be formatted.
        /// </param>
        /// <param name="showHeader">
        /// Should a header describing race location be shown or not?
        /// </param>
        public IEnumerable<string> Format(IRace race, bool showHeader)
        {
            // Convert to list to ensure that enumeration happens only once
            IEnumerable<string> resultsLines = ResultsFormatter.Format(race.Gender, race.Distance, race.Results).ToList();
            int width = resultsLines.Max(line => line.Length);
            if(showHeader)
            {
                yield return StringFormatting.Centered(race.Meet.Name, width);
                yield return StringFormatting.Centered(race.Date.ToString(), width);
                yield return StringFormatting.Centered(race.Meet.Venue, width);
                yield return "";
            }
            foreach(string line in resultsLines)
            {
                yield return line;
            }
            yield return "";
            foreach(string line in ScoreFormatter.Format(race.Scores.OrderBy(score => score)))
            {
                yield return line;
            }
        }

        #endregion
    }
}
