using System;
using UnityEngine;

namespace Moonstorm
{
    public abstract class CharacterBase : ContentBase
    {
        public abstract GameObject BodyPrefab { get; set; }
        public abstract GameObject MasterPrefab { get; set; }

        public override void Initialize()
        {
            ModifyPrefab();
            Hook();
        }

        public virtual void ModifyPrefab() { }
        public virtual void Hook() { }
    }
}