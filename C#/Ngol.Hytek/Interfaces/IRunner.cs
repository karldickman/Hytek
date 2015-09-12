using System;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// Interface to which runners must conform.
    /// </summary>
    public interface IRunner
    {
        /// <summary>
        /// The year the runner enrolled in college.
        /// </summary>
        int? GraduationYear { get; }

        /// <summary>
        /// The name of the runner.
        /// </summary>
        string Name { get; }
    }
}

