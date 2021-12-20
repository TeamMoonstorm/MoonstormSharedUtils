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
        /// <para>Use "AddRequiredType" for adding dependencies</para>
        /// </summary>
        public Type[] RequiredTypes { get; private set; } = Array.Empty<Type>();

        /// <summary>
        /// Adds a ContentBase inheriting class as a dependency for this unlockable to be added to the game
        /// </summary>
        /// <typeparam name="T">The class that needs to be initialized before the unlockable gets initialized</typeparam>
        protected void AddRequiredType<T>() where T : ContentBase
        {
            var list = RequiredTypes.ToList();
            list.Add(typeof(T));
            RequiredTypes = list.ToArray();
        }

        /// <summary>
        /// LateInitialization gets called when the unlockable has passed the RequiredTypes check.
        /// <para>Any final initialization should be done here</para>
        /// </summary>
        public virtual void LateInitialization() { }

        /// <summary>
        /// Gets the AchievementDef that corresponds to this Unlockable
        /// </summary>
        public AchievementDef GetAchievementDef { get => UnlockableDef.AchievementDef; }
    }
}
