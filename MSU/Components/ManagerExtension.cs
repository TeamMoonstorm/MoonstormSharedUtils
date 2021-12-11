using RoR2;
using UnityEngine;

namespace Moonstorm.Components
{
    public class ManagerExtension : MonoBehaviour
    {
        protected int extensionIndex;
        public MoonstormItemManager manager;
        public CharacterBody body;

        internal void SetIndex(int index) => extensionIndex = index;
        public virtual void GetInterfaces() { }

        public virtual void CheckForItems() { }

        public virtual void CheckForBuffs() { }
    }
}
