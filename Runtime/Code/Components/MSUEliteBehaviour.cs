using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// The EliteBehaviour is used for managing the extra metadata found in a <see cref="ExtendedEliteDef"/> and applying said metadata to a CharacterBodys
    /// </summary>
    public sealed class MSUEliteBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The CharacterBody assigned to this EliteBehaviour
        /// </summary>
        public CharacterBody body;
        /// <summary>
        /// The <see cref="body"/>'s CharacterModel
        /// </summary>
        public CharacterModel characterModel;

        private GameObject _effectInstance;
        private EliteIndex _assignedIndex = EliteIndex.None;

        internal void AssignNewIndex(EliteIndex index)
        {
            //Incoming index is same, return early.
            if (_assignedIndex == index)
                return;

            _assignedIndex = index;
            //destroy effect if it exists because we're no longer an elite.
            if(_assignedIndex == EliteIndex.None)
            {
                if (_effectInstance)
                    Destroy(_effectInstance);
                return;
            }

            //We're being assigned a new index, destroy the effect then create the new one if it exists..
            if (_effectInstance)
                Destroy(_effectInstance);

            if(EquipmentModule.eliteIndexToEffectPrefab.TryGetValue(index, out var prefab))
            {
                _effectInstance = Instantiate(prefab, characterModel ? characterModel.transform : body ? body.transform : transform, false);
            }
        }

        private void OnDestroy()
        {
            if (_effectInstance)
                Destroy(_effectInstance);
        }
    }
}
