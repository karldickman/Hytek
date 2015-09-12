using System;
using System.Collections.Generic;

namespace Ngol.Hytek.Interfaces
{
    /// <summary>
    /// The interface to which all formatters must adhere.
    /// </summary>
    public interface IFormatter<T>
    {
        /// <summary>
        /// Format a value into a list of lines.
        /// </summary>
        /// <param name="thing">
        /// The value to format.
        /// </param>
        /// <returns>
        /// A sequence of lines.
        /// </returns>
        IEnumerable<string> Format(T thing);
    }
}

