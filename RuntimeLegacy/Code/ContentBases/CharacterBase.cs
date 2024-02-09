using UnityEngine;

namespace Moonstorm
{
    public abstract class CharacterBase : ContentBase
    {
        public abstract GameObject BodyPrefab { get; }

        public abstract GameObject MasterPrefab { get; }

        public override void Initialize()
        {
            ModifyPrefab();
            Hook();
        }

        public virtual void ModifyPrefab() { }

        public virtual void Hook() { }
    }
}