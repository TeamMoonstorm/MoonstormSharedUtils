using RoR2;
using UnityEngine;

namespace Moonstorm.Components
{
    /// <summary>
    /// A class to derive a MoonstormItemManager Extension
    /// </summary>
    public class ManagerExtension : MonoBehaviour
    {
        //This extension's index
        protected int extensionIndex;
        /// <summary>
        /// A direct access to the manager that's attatched to the body
        /// </summary>
        public MoonstormItemManager manager;
        /// <summary>
        /// The body that this component is attatched to
        /// </summary>
        public CharacterBody body;

        internal void SetIndex(int index) => extensionIndex = index;

        /// <summary>
        /// Obtain any custom interfaces here.
        /// Gets called once the main manager's GetInterfaces method ends
        /// </summary>
        public virtual void GetInterfaces() { }

        /// <summary>
        /// Check for any custom Item/Equipment related logic here
        /// Gets called once the main manager's CheckForItems method ends
        /// </summary>
        public virtual void CheckForItems() { }

        /// <summary>
        /// Check for any Buff related logic here
        /// Gets called once the main manager's CheckForBuffs method ends
        /// </summary>
        public virtual void CheckForBuffs() { }
    }
}
