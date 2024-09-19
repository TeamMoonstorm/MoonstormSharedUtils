using R2API;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    public abstract class InteractableBase : ContentBase
    {
        public delegate bool IsAvailableForDCCSDelegate(DirectorAPI.StageInfo stageInfo);

        public abstract GameObject Interactable { get; }

        public virtual List<MSInteractableDirectorCard> InteractableDirectorCards { get; } = new List<MSInteractableDirectorCard>();

        public virtual IsAvailableForDCCSDelegate IsAvailableForDCCS { get; } = DefaultIsAvailable;

        private static bool DefaultIsAvailable(DirectorAPI.StageInfo stageInfo) => true;
    }
}