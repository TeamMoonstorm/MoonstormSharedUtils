using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents a CharacterBody and Master pair for the game
    /// <para>The CharacterBody is represented via the <see cref="BodyPrefab"/>, while the CharacterMaster is represented via the <see cref="MasterPrefab"/></para>
    /// <para>It's tied ModuleBase is the <see cref="CharacterModuleBase"/></para>
    /// <para>You should also see <seealso cref="MonsterBase"/> and the <seealso cref="SurvivorBase"/> for implementing Monsters and Survivors respectively</para>
    /// </summary>
    public abstract class CharacterBase : ContentBase
    {
        /// <summary>
        /// The CharacterBody prefab associated with this CharacterBase
        /// </summary>
        public abstract GameObject BodyPrefab { get; }
        /// <summary>
        /// The CharacterMaster prefab aassociated with this CharacterBase
        /// </summary>
        public abstract GameObject MasterPrefab { get; }

        /// <summary>
        /// <inheritdoc cref="ContentBase.Initialize"/>
        /// <para>calling base also calls <see cref="ModifyPrefab"/> and <see cref="Hook"/></para>
        /// </summary>
        public override void Initialize()
        {
            ModifyPrefab();
            Hook();
        }

        /// <summary>
        /// Finalize your prefab here
        /// </summary>
        public virtual void ModifyPrefab() { }

        /// <summary>
        /// Implement hooks for your character to work properly here
        /// </summary>
        public virtual void Hook() { }
    }
}