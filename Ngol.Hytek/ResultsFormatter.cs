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
        public ResultsFormatter() : base(NewDataTable())
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create the new <see cref="DataTable" /> to be formatted.
        /// </summary>
        protected static DataTable NewDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("", typeof(int));
            table.Columns.Add("Name", typeof(IRunner));
            table.Columns.Add("Year", typeof(int));
            table.Columns.Add("School", typeof(ITeam));
            table.Columns.Add("Finals", typeof(double));
            table.Columns.Add("Points", typeof(int?));
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
            IEnumerable<Func<object, int, string >> alignments = new Func<object, int, string>[] {
                StringFormatting.RightJustified,
                (object runner, int width) =>
                    StringFormatting.RightJustified(((IRunner)runner).Name, width),
                StringFormatting.LeftJustified,
                (object team, int width) =>
                    StringFormatting.LeftJustified(team == null ? string.Empty : ((ITeam)team).Name, width),
                (object unsafeTime, int width) =>
                {
                    double? time = (double?)unsafeTime;
                    return StringFormatting.LeftJustified(time.HasValue ? FormatTime(time.Value) : "DNF", width);
                },
                (object points, int width) =>
                    StringFormatting.RightPadded(points, 3) + "   ",
            };
            results.ForEachIndexed(1, (result, place) =>
            {
                Table.Rows.Add(place, result.Runner, result.Runner.EnrollmentYear, result.Team, result.Time, result.Points);
            });
            return base.Format(alignments);
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

        #endregion
    }
}
