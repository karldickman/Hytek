using System;
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>
        /// Create a new formatter.
        /// </summary>
        public ResultsFormatter() : base(new string[] { null, "Name", "Year", "School", "Finals", "Points" })
        {
        }

        /// <summary>
        /// The alignment used for points.
        /// </summary>
        /// <param name="points">
        /// A <see cref="System.Object"/>.  The points the runner got.
        /// </param>
        /// <param name="width">
        /// A <see cref="System.Int32"/>.  The width of the desired string.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/>.  If points is wider than width, this
        /// is the same as points.ToString().
        /// </returns>
        public static string AlignPoints(object points, int width)
        {
            return StringFormatting.RightPadded(points, 3) + "   ";
        }

        /// <summary>
        /// Format the results into a table.
        /// </summary>
        /// <param name="results">
        /// A sequence of results.
        /// </param>
        public IEnumerable<string> Format(IOrderedEnumerable<IPerformance> results)
        {
            IEnumerable<Alignment> alignments = new Alignment[] { StringFormatting.RightJustified, null, null, null, null, AlignPoints };
            IEnumerable<IEnumerable<object>> values = results.Select(1, (result, place) =>
            {
                ICollection<string> valueRow = new List<string> {
                    place.ToString(),
                    result.Runner.Name,
                    result.Runner.EnrollmentYear.ToString(),
                };
                valueRow.Add(result.Team == null ? string.Empty : result.Team.Name);
                if(result.Time != null)
                {
                    valueRow.Add(FormatTime(result.Time.Value));
                    valueRow.Add(result.Points.ToString());
                }
                else
                {
                    valueRow.Add("DNF");
                    valueRow.Add(string.Empty);
                }
                return valueRow.Cast<object>();
            });
            foreach(string line in base.Format(values, alignments))
            {
                yield return line;
            }
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
    }
}
