using System;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// Useful extensions on <see cref="IPerformance" />.
    /// </summary>
    public static class IPerformanceExtensions
    {
        /// <summary>
        /// The pace in minutes per mile of the performance.
        /// </summary>
        /// <param name="performance">
        /// The <see cref="IPerformance" /> whose mile pace to calcuate.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="performance"/> is <see langword="null" />.
        /// </exception>
        public static double? GetMilePace(this IPerformance performance)
        {
            if(performance == null)
                throw new ArgumentNullException("performance");
            if(performance.Time == null)
            {
                return null;
            }
            return performance.Time.Value / performance.RaceDistance * 60 * 1609.344;
        }
    }
}

