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
        /// The interactable prefab for the InteractableBase
        /// </summary>
        public abstract GameObject Interactable { get; }

        [Obsolete("Use the InteractableDirectorCards list instead.")]
        public virtual MSInteractableDirectorCard InteractableDirectorCard { get; }
        /// <summary>
        /// A list of <see cref="MSInteractableDirectorCard"/> used for spawning the interactable prefab.
        /// </summary>
        public virtual List<MSInteractableDirectorCard> InteractableDirectorCards { get; } = new List<MSInteractableDirectorCard>();
    }
}