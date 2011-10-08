using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ngol.Hytek.Interfaces;
using Ngol.Utilities.Collections.Extensions;
using Ngol.Utilities.TextFormat;

namespace Ngol.Hytek
{
    /// <summary>
    /// A formatter for the team scores of a race.
    /// </summary>
    public class ScoreFormatter : HytekFormatter, IFormatter<IOrderedEnumerable<ITeamScore>>
    {
        #region Constructors

        /// <summary>
        /// Create a new formatter.
        /// </summary>
        public ScoreFormatter() : base("Team Scores", new string[] { "Rank", "Team", "Total", "   1", "   2", "   3", "   4", "   5", "  *6", "  *7" })
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format the team scores.
        /// </summary>
        /// <param name="scores">
        /// The scores to format.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="scores"/> is <see langword="null" />.
        /// </exception>
        public IEnumerable<string> Format(IOrderedEnumerable<ITeamScore> scores)
        {
            if(scores == null)
                throw new ArgumentNullException("scores");
            Alignment R = StringFormatting.RightJustified;
            IEnumerable<Alignment> alignments = new List<Alignment> { R, null, R, R, R, R, R, R, R, R };
            IEnumerable<IEnumerable<object>> values = scores.SelectMany<ITeamScore, IEnumerable<object>>(1, FormatTeamScore);
            return base.Format(values, alignments);
        }

        private IEnumerable<IEnumerable<object>> FormatTeamScore(ITeamScore score, int place)
        {
            ICollection<string> valueRow = new List<string>();
            valueRow.Add(score.Score.HasValue ? place.ToString() : null);
            valueRow.Add(score.Team.Name);
            valueRow.Add(score.Score.ToString());
            score.Performances.Take(7).ForEachIndexed((runner, j) =>
            {
                if(j < score.Performances.Count())
                {
                    valueRow.Add(runner.Points.ToString());
                }
                else
                {
                    valueRow.Add(null);
                }
            });
            yield return valueRow.Cast<object>();
            valueRow = new List<string> {
                "  Top 5 Avg: " + FormatTime(score.TopFiveAverage),
            };
            yield return valueRow.Cast<object>();
            if(score.Performances.Count() > 5)
            {
                valueRow = new List<string> {
                    "  Top 7 Avg: " + FormatTime(score.TopSevenAverage),
                };
                yield return valueRow.Cast<object>();
            }
        }

        #endregion
    }
}
