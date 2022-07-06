using RoR2;
using System;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents a <see cref="RoR2.UnlockableDef"/> and <see cref="RoR2.AchievementDef"/> pair for the game.
    /// <para>Both the <see cref="RoR2.UnlockableDef"/> and <see cref="RoR2.AchievementDef"/> are represented via the <see cref="UnlockableDef"/></para>
    /// <para>Its tied ModuleBase is the <see cref="UnlockablesModuleBase"/></para>
    /// <para>More information about MSU's unlockables system can be found in the documentation for <seealso cref="MSUnlockableDef"/>"/></para>
    /// </summary>
    public abstract class UnlockableBase : ContentBase
    {
        /// <summary>
        /// The MSUnlockableDef associated with this UnlockableBase
        /// </summary>
        public abstract MSUnlockableDef UnlockableDef { get; }
        /// <summary>
        /// An array of required <see cref="ContentBase"/>s that need to be initialized for this unlockable to be added. Types are added via <see cref="AddRequiredType{T}"/>
        /// <para>For example, Adding an UnlockableDef for an Item that can be disabled or enabled in a config file. You'd only want this unlockable to be implemented if the Item is enabled in the config file, as such, Adding a required type of the item that gets unlocked when this Unlockable is obtained means this Unlockable only gets added if the Required Type is also added.</para>
        /// </summary>
        public Type[] RequiredTypes { get; private set; } = Array.Empty<Type>();

        /// <summary>
        /// Adds a new RequiredType to this UnlockableBase.
        /// <para>For example, Adding an UnlockableDef for an Item that can be disabled or enabled in a config file, the Item in this case would be <typeparamref name="T"/>. You'd only want this unlockable to be implemented if the Item (<typeparamref name="T"/>) is enabled in the config file, as such, Adding a required type of the item that gets unlocked when this Unlockable is obtained means this Unlockable only gets added if the Required Type is also added.</para>
        /// </summary>
        /// <typeparam name="T">The type that is required for adding this Unlockable to the game.</typeparam>
        protected void AddRequiredType<T>() where T : ContentBase
        {
            var list = RequiredTypes.ToList();
            list.Add(typeof(T));
            RequiredTypes = list.ToArray();
        }

        /// <summary>
        /// Called when all the required types for this unlockable are enabled.
        /// </summary>
        public virtual void OnCheckPassed() { }

        /// <summary>
        /// The AchievementDef tied to this Unlockable
        /// </summary>
        public AchievementDef GetAchievementDef { get => UnlockableDef.AchievementDef; }
    }
}
