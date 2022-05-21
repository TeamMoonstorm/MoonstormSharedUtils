using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm
{
    [Flags]
    public enum EventFlags : uint
    {
        WeatherRelated = 1U,
        AfterVoidFields = 2U,
        AfterLoop = 4u,
        EnemySpawn = 8u,
        OncePerRun = 16u,
    }
}
