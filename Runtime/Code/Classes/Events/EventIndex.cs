using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// An event index, get this from <see cref="EventCard.EventIndex"/> directly or via <see cref="EventCatalog.FindEventIndex(string)"/>
    /// </summary>
    public enum EventIndex
    {
        /// <summary>
        /// Represents an invalid event.
        /// </summary>
        None = -1
    }
}
