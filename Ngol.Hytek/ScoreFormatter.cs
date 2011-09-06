using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ngol.Hytek.Interfaces;
using Ngol.Utilities.TextFormat;

namespace Ngol.Hytek
{
    /// <summary>
    /// A formatter for the team scores of a race.
    /// </summary>
    public class ScoreFormatter : HytekFormatter, IFormatter<IOrderedEnumerable<ITeamScore>>
    {
        /// <summary>
        /// Create a new formatter.
        /// </summary>
        public ScoreFormatter() : base("Team Scores", new string[] { "Rank", "Team", "Total", "   1", "   2", "   3", "   4", "   5", "  *6", "  *7" })
        {
        }

        /// <summary>
        /// Format the team scores.
        /// </summary>
        /// <param name="scores">
        /// The scores to format.
        /// </param>
        public IEnumerable<string> Format(IOrderedEnumerable<ITeamScore> scores)
        {
            Alignment R = StringFormatting.RightJustified;
            Alignment[] alignments = new Alignment[] { R, null, R, R, R, R, R, R, R, R };
            IPerformance runner;
            IList<IList> values = new List<IList>();
            int i = 0;
            foreach(ITeamScore score in scores)
            {
                IList valueRow = new ArrayList();
                if(score.Score != null)
                {
                    valueRow.Add(i + 1);
                }
                else
                {
                    valueRow.Add(null);
                }
                valueRow.Add(score.Team.Name);
                valueRow.Add(score.Score);
                for(int j = 0; j < 7; j++)
                {
                    if(j < score.Performances.Count)
                    {
                        runner = score.Performances[j];
                        valueRow.Add(runner.Points);
                    }

                    else
                    {
                        valueRow.Add(null);
                    }
                }
                values.Add(valueRow);
                valueRow = new object[Header.Count];
                valueRow[1] = "  Top 5 Avg: ";
                valueRow[1] += FormatTime(score.TopFiveAverage);
                values.Add(valueRow);
                if(score.Performances.Count > 5)
                {
                    valueRow = new object[Header.Count];
                    valueRow[1] = "  Top 7 Avg: ";
                    valueRow[1] += FormatTime(score.TopSevenAverage);
                    values.Add(valueRow);
                }
                i++;
            }
            return base.Format(values, alignments);
        }
    }
}
