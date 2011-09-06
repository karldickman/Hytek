using System;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// The interface to which teams must conform.
    /// </summary>
    public interface ITeam
    {
        /// <summary>
        /// The name of the team.
        /// </summary>
        string Name { get; }
    }
}

