using System;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing character
    /// <para>If you want to initialize a survivor, you should look at SurvivorBase</para>
    /// <para>If you want to initialize a monster, you should look at MonsterBase</para>
    /// </summary>
    public abstract class CharacterBase
    {
        /// <summary>
        /// The Body Prefab of your Character
        /// </summary>
        public abstract GameObject BodyPrefab { get; set; }
        /// <summary>
        /// The MasterPrefab of your character
        /// </summary>
        public abstract GameObject MasterPrefab { get; set; }

        /// <summary>
        /// Initialize your Character here
        /// <para>calling base.Initialize() heavily reccomended.</para>
        /// </summary>
        public void Initialize()
        {
            ModifyPrefab();
            Hook();
        }

        /// <summary>
        /// Modify your Prefabs here
        /// </summary>
        public virtual void ModifyPrefab() { }
        [Obsolete("No longer used, consider registering your tokens via your mod's language.lang file")]
        public virtual void RegisterTokens() { }
        /// <summary>
        /// Use hooks here.
        /// </summary>
        public virtual void Hook() { }
    }
}