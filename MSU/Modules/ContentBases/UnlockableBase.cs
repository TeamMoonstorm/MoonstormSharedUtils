using RoR2;
using System;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing an Unlockable
    /// </summary>
    public abstract class UnlockableBase : ContentBase
    {
        /// <summary>
        /// Your Unlockable's unlockableDef
        /// </summary>
        public abstract MSUnlockableDef UnlockableDef { get; set; }

        /// <summary>
        /// A collection of  that need to be initialized before the unlockable is added to the game.
        /// </summary>
        public Type[] RequiredTypes { get; private set; } = Array.Empty<Type>();

        protected void AddRequiredType<T>() where T : ContentBase
        {
            var list = RequiredTypes.ToList();
            list.Add(typeof(T));
            RequiredTypes = list.ToArray();
        }
        public AchievementDef GetAchievementDef { get => UnlockableDef.AchievementDef; }
    }
}
