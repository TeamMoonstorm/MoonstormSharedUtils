using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing an Interactable
    /// </summary>
    public abstract class InteractableBase : ContentBase
    {
        /// <summary>
        /// The main prefab of your Interactable
        /// </summary>
        public abstract GameObject Interactable { get; set; }

        /// <summary>
        /// And MSInteractableDirectorCard that spawns your interactable
        /// <para>Used for the SceneDirector</para>
        /// </summary>
        public virtual MSInteractableDirectorCard InteractableDirectorCard { get; set; }
    }
}