using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ngol.Utilities.TextFormat;
using Ngol.Utilities.TextFormat.Table;

namespace Ngol.Hytek
{
    /// <summary>
    /// A formatter that produces Hytek-style tables.
    /// </summary>
    public class HytekFormatter
    {
        #region Properties

        /// <summary>
        /// The Header of the hytek table.
        /// </summary>
        public IEnumerable<string> Header
        {
            get;
            set;
        }

        /// <summary>
        /// The title of the table.
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// The table formatter used internally.
        /// </summary>
        public LabeledTableFormatter TableFormatter
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a formatter that produces tables without titles.
        /// </summary>
        /// <param name="header">
        /// A <see cref="System.String[]"/>.  The header of the table.
        /// </param>
        public HytekFormatter(IList<string> header) : this(null, header)
        {
        }

        /// <summary>
        /// Create a formatter that produces tables with titles.
        /// </summary>
        /// <param name="title">
        /// A <see cref="System.String"/>.  The title of the table.
        /// </param>
        /// <param name="header">
        /// A <see cref="System.String[]"/>.  The header of the table.
        /// </param>
        public HytekFormatter(string title, IList<string> header)
        {
            Title = title;
            Header = header;
            TableFormatter = new LabeledTableFormatter('\0', ' ', '\0', '=', '\0', '=', '\0', '=');
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format the specified seconds into a time in minutes and seconds.
        /// </summary>
        /// <param name="time">
        /// The time to format.
        /// </param>
        public static string FormatTime(double time)
        {
            int minutes = (int)(time / 60);
            return string.Format("{0}:{1:00.00}", minutes, time - minutes * 60);
        }

        /// <summary>
        /// Format a list of values.
        /// </summary>
        /// <param name="values">
        /// A sequence of values to format.
        /// </param>
        /// <param name="alignments">
        /// The alignments of the columns.
        /// </param>
        protected IEnumerable<string> Format(IEnumerable<IEnumerable<object>> values, IEnumerable<Alignment> alignments)
        {
            IEnumerable<string> lines = TableFormatter.Format(Header.Cast<object>(), values, alignments);
            if(Title != null)
            {
                yield return StringFormatting.Centered(Title, lines.Max(line => lines.Count()));
            }
            foreach(string line in lines)
            {
                yield return line;
            }
        }

        #endregion
    }
}
