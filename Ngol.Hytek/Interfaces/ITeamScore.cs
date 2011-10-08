using System;
using System.Collections.Generic;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// Interface to which team scores must conform.
    /// </summary>
    public interface ITeamScore : IComparable<ITeamScore>
    {
        /// <summary>
        /// The number of runners on the team who finished.
        /// </summary>
        int FinisherCount
        {
            get;
        }

        /// <summary>
        /// The runners on the team.
        /// </summary>
        IEnumerable<IPerformance> Performances
        {
            get;
        }

        /// <summary>
        /// The total points earned by the team.
        /// </summary>
        int? Score
        {
            get;
        }

        /// <summary>
        /// The team that earned the score.
        /// </summary>
        ITeam Team
        {
            get;
        }

        /// <summary>
        /// The average of the top five times.
        /// </summary>
        double TopFiveAverage
        {
            get;
        }

        /// <summary>
        /// The average of the top seven times.
        /// </summary>
        double TopSevenAverage
        {
            get;
        }
    }
}

