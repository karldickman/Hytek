using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

        #region Physical implementation

        private Table _table;

        #endregion

        /// <summary>
        /// The <see cref="DataTable" /> underlying this table.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if an attempt is made to set this property to <see langword="null" />.
        /// </exception>
        public Table Table
        {
            get { return _table; }

            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _table = value;
            }
        }

        /// <summary>
        /// The title of the <see cref="Table" />.
        /// </summary>
        public string Title
        {
            get { return Table.Title; }

            protected set { Table.Title = value; }
        }

        /// <summary>
        /// The <see cref="TableFormatter" /> used internally.
        /// </summary>
        protected readonly LabeledTableFormatter TableFormatter;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a formatter that produces tables without titles.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable" /> to format.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="table"/> is <see langword="null" />.
        /// </exception>
        public HytekFormatter(Table table)
        {
            if(table == null)
            {
                throw new ArgumentNullException("table");
            }
            Table = table;
            TableFormatter = new LabeledTableFormatter('\0', ' ', '\0', '=', '\0', '=', '=');
        }

        #endregion

        #region Methods

        /// <summary>
        /// Format the <see cref="Table" />.
        /// </summary>
        protected IEnumerable<string> Format()
        {
            IEnumerable<string > lines = TableFormatter.Format(Table);
            if(Title != null)
            {
                yield return StringFormatting.Centered(Title, lines.Max(line => lines.Count()));
            }
            foreach(string line in lines)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Format the specified seconds into a time in minutes and seconds.
        /// </summary>
        /// <param name="time">
        /// The time to format.
        /// </param>
        protected static string FormatTime(double time)
        {
            int minutes = Convert.ToInt32(Math.Floor(time / 60));
            double seconds = time - minutes * 60;
            return string.Format("{0}:{1:00.00}", minutes, seconds);
        }

        #endregion
    }
}
