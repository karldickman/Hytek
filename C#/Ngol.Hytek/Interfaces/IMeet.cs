using System;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// Interface to which meet instances must conform.
    /// </summary>
    public interface IMeet
    {
        /// <summary>
        /// The name of the meet.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// The venue whereat this meet was held.
        /// </summary>
        string Venue { get; }
    }
}

