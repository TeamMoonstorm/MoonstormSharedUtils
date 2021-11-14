using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
