using System;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// Interface to which performances must conform.
    /// </summary>
    public interface IPerformance : IComparable<IPerformance>
    {
        /// <summary>
        /// The points awarded for the performance.
        /// </summary>
        int? Points
        {
            get;
        }

        /// <summary>
        /// The length of the race in meters.
        /// </summary>
        int RaceDistance
        {
            get;
        }

        /// <summary>
        /// The runner who achieved the performance.
        /// </summary>
        IRunner Runner
        {
            get;
        }

        /// <summary>
        /// The team for whom the runner was running.
        /// </summary>
        ITeam Team
        {
            get;
        }

        /// <summary>
        /// The time, in seconds, the runner ran.
        /// </summary>
        double? Time
        {
            get;
        }
    }
}

