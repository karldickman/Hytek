using System;
using System.Collections.Generic;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// Interface to which all races must conform.
    /// </summary>
    public interface IRace
    {
        /// <summary>
        /// The date on which the race was held.
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        /// The length of the race in meters.
        /// </summary>
        int Distance { get; }

        /// <summary>
        /// Was this a men's race or a women's race?
        /// </summary>
        Gender Gender
        {
            get;
        }

        /// <summary>
        /// The meet instance of which this race was a part.
        /// </summary>
        IMeet Meet { get; }

        /// <summary>
        /// The results of the race.
        /// </summary>
        IEnumerable<IPerformance> Results
        {
            get;
        }

        /// <summary>
        /// The team scores for the race.
        /// </summary>
        IEnumerable<ITeamScore> Scores { get; }
    }
}

