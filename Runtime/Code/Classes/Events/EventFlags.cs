using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    /// <summary>
    /// Represents flags for events.
    /// </summary>
    [Flags]
    public enum EventFlags : uint
    {
        /// <summary>
        /// This event needs to run on the exclusive "Weather" entity state machine
        /// </summary>
        WeatherRelated = 1U,
        /// <summary>
        /// This event can only run after void fields
        /// </summary>
        AfterVoidFields = 2U,
        /// <summary>
        /// This event can only run after looping at least once
        /// </summary>
        AfterLoop = 4u,
        /// <summary>
        /// This event spawns enemies
        /// </summary>
        EnemySpawn = 8u,
        /// <summary>
        /// This event can only happen once per run
        /// </summary>
        OncePerRun = 16u,
    }
}
