using System;

namespace Moonstorm
{
    [Obsolete]
    [Flags]
    public enum EventFlags : uint
    {
        AfterVoidFields = 2U,
        AfterLoop = 4u,
        EnemySpawn = 8u,
        OncePerRun = 16u,
    }
}
