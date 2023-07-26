using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="ContentBase"/> that represents an Interactable for the game. The interactable is represented via the <see cref="Interactable"/>
    /// <para>It's tied ModuleBase is the <see cref="InteractableModuleBase"/></para>
    /// <para>An InteractableBase can have an <see cref="MSInteractableDirectorCard"/>, which is used for spawning the interactable ingame with the Scene Director</para>
    /// </summary>
    public abstract class InteractableBase : ContentBase
    {
        /// <summary>
        /// Represents if the Interactable is available for a DCCS
        /// </summary>
        /// <returns>true if the Interactable should be added, false otherwise</returns>
        public delegate bool IsAvailableForDCCSDelegate();
        /// <summary>
        /// The interactable prefab for the InteractableBase
        /// </summary>
        public abstract GameObject Interactable { get; }

        [Obsolete("Use the InteractableDirectorCards list instead.")]
        public virtual MSInteractableDirectorCard InteractableDirectorCard { get; }
        /// <summary>
        /// A list of <see cref="MSInteractableDirectorCard"/> used for spawning the interactable prefab.
        /// </summary>
        public virtual List<MSInteractableDirectorCard> InteractableDirectorCards { get; } = new List<MSInteractableDirectorCard>();

        /// <summary>
        /// Whenever the DCCS are being modified to have the custom monster, this delegate is invoked, return True if you want the monster to be added, or False if you dont want the monster to be added.
        /// </summary>
        public virtual IsAvailableForDCCSDelegate IsAvailableForDCCS { get; } = DefaultIsAvailable;

        private static bool DefaultIsAvailable() => true;
    }
}