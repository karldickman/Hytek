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
    /// A formatter for the list of times in a race.
    /// </summary>
    public class ResultsFormatter : HytekFormatter, IFormatter<IOrderedEnumerable<IPerformance>>
    {
        #region Constructors

        /// <summary>
        /// Create a new formatter.
        /// </summary>
        public ResultsFormatter() : base(NewTable())
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create the new <see cref="DataTable" /> to be formatted.
        /// </summary>
        protected static Table NewTable()
        {
            Table table = new Table();
            table.Columns.Add(" ", typeof(int), alignment: StringFormatting.RightJustified);
            table.Columns.Add("Name", typeof(IRunner), FormatRunner);
            table.Columns.Add("Year", typeof(int?));
            table.Columns.Add("School", typeof(ITeam), FormatTeam);
            table.Columns.Add("Finals", typeof(double), FormatTime);
            table.Columns.Add("Points", typeof(int?), FormatPoints);
            return table;
        }

        /// <summary>
        /// Format the results into a table.
        /// </summary>
        /// <param name="results">
        /// A sequence of results.
        /// </param>
        public IEnumerable<string> Format(IOrderedEnumerable<IPerformance> results)
        {
            results.ForEachIndexed(1, (result, place) =>
            {
                Table.Rows.Add(place, result.Runner, result.Runner.GraduationYear, result.Team, result.Time, result.Points);
            });
            return base.Format();
        }

        /// <summary>
        /// Format results in the table.
        /// </summary>
        /// <param name="gender">
        /// Was this a men's or a women's race?
        /// </param>
        /// <param name="distance">
        /// The length of the race in meters.
        /// </param>
        /// <param name="results">
        /// A sequence of results.
        /// </param>
        public IEnumerable<string> Format(Gender gender, int distance, IEnumerable<IPerformance> results)
        {
            Title = string.Format("{0} {1} m run CC", gender == Gender.Male ? "Men's " : "Women's ", distance);
            return Format(results.Sorted());
        }

        private static string FormatRunner(object runner)
        {
            return FormatRunner(runner as IRunner);
        }

        private static string FormatRunner(IRunner runner)
        {
            if(runner == null)
            {
                throw new ArgumentNullException("runner");
            }
            return runner.Name;
        }

        private static string FormatPoints(object points)
        {
            return FormatPoints(points as int?);
        }

        private static string FormatPoints(int? points)
        {
            return points.HasValue ? StringFormatting.RightPadded(points.Value, 3) : string.Empty;
        }

        private static string FormatTeam(object team)
        {
            return FormatTeam(team as ITeam);
        }

        private static string FormatTeam(ITeam team)
        {
            return team == null ? string.Empty : team.Name;
        }

        private static string FormatTime(object time)
        {
            return FormatTime(time as double?);
        }

        private static string FormatTime(double? time)
        {
            return time.HasValue ? HytekFormatter.FormatTime(time.Value) : "DNF";
        }

        #endregion
    }
}
