using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ngol.Hytek.Interfaces;
using Ngol.Utilities.TextFormat;

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
            Alignment[] alignments = new Alignment[] { StringFormatting.RightJustified, null, null, null, null, AlignPoints };
            IList<IList> values = new List<IList>();
            int i = 0;
            foreach(IPerformance result in results)
            {
                object[] valueRow = new object[Header.Count];
                int n = 0;
                valueRow[n++] = (i + 1).ToString();
                valueRow[n++] = result.Runner.Name;
                valueRow[n++] = result.Runner.EnrollmentYear.ToString();
                if(result.Team == null)
                {
                    valueRow[n++] = "";
                }

                else
                {
                    valueRow[n++] = result.Team.Name;
                }
                if(result.Time != null)
                {
                    valueRow[n++] = FormatTime(result.Time.Value);
                    valueRow[n++] = result.Points.ToString();
                }

                else
                {
                    valueRow[n++] = "DNF";
                    valueRow[n++] = "";
                }
                values.Add(valueRow);
                i++;
            }
            return base.Format(values, alignments);
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
            Title = gender == Gender.Male ? "Men's " : "Women's ";
            Title += distance + " m run CC";
            return Format(results.OrderBy(performance => performance));
        }
    }
}
