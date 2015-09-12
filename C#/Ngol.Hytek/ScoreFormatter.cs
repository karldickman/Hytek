using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ngol.Hytek.Interfaces;
using Ngol.Utilities.Collections.Extensions;
using Ngol.Utilities.TextFormat;
using Ngol.Utilities.TextFormat.Table;

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
        public ScoreFormatter() : base(NewTable("Team Scores"))
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create the new <see cref="DataTable" /> to be formatted.
        /// </summary>
        /// <param name="tableName">
        /// The name to give the <see cref="DataTable" />.
        /// </param>
        protected static Table NewTable(string tableName)
        {
            Table table = new Table(tableName);
            table.Columns.Add("Rank", typeof(int?), alignment: StringFormatting.RightJustified);
            table.Columns.Add("Team", typeof(string), alignment: StringFormatting.LeftJustified);
            table.Columns.Add("Total", alignment: StringFormatting.RightJustified);
            table.Columns.Add("   1", typeof(int?), alignment: StringFormatting.RightJustified);
            table.Columns.Add("   2", typeof(int?), alignment: StringFormatting.RightJustified);
            table.Columns.Add("   3", typeof(int?), alignment: StringFormatting.RightJustified);
            table.Columns.Add("   4", typeof(int?), alignment: StringFormatting.RightJustified);
            table.Columns.Add("   5", typeof(int?), alignment: StringFormatting.RightJustified);
            table.Columns.Add("  *6", typeof(int?), alignment: StringFormatting.RightJustified);
            table.Columns.Add("  *7", typeof(int?), alignment: StringFormatting.RightJustified);
            return table;
        }

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
            {
                throw new ArgumentNullException("scores");
            }
            scores.ForEachIndexed(1, (score, placeIndex) =>
            {
                int?[] points = new int?[7];
                score.Performances.Take(7).ForEachIndexed((runner, i) =>
                {
                    points[i] = runner.Points;
                });
                int? place = score.HasScore ? placeIndex : (int?)null;
                Table.Rows.Add(place, score.Team, score.Score, points[0], points[1], points[2], points[3], points[4], points[5], points[6]);
                Table.Rows.Add(null, "  Top 5 Avg: " + FormatTime(score.TopFiveAverage));
                Table.Rows.Add(null, "  Top 7 Avg: " + FormatTime(score.TopSevenAverage));
            });
            return base.Format();
        }

        #endregion
    }
}
