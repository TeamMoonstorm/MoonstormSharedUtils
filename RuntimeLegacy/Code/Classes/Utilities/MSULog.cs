using BepInEx.Logging;
using Moonstorm.Loaders;
using System.Runtime.CompilerServices;

namespace Moonstorm
{
    internal class MSULog : LogLoader<MSULog>
    {
        public override ManualLogSource LogSource { get; protected set; }

        public override BreakOnLog BreakOn => BreakOnLog.Fatal;

        public MSULog(ManualLogSource logSource) : base(logSource)
        {
        }
    }
}
