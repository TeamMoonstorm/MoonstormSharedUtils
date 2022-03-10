using UnityEngine;

namespace Moonstorm
{
    public abstract class InteractableBase : ContentBase
    {
        public abstract GameObject Interactable { get; }

        public virtual MSInteractableDirectorCard InteractableDirectorCard { get; }
    }
}