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

        private Dictionary<BuffIndex, GameObject> _eliteBuffIndexToEffectInstance = new Dictionary<BuffIndex, GameObject>();

        internal void OnEliteBuffFirstStackGained(BuffIndex eliteBuffIndex, ExtendedEliteDef eed)
        {
            if (!_eliteBuffIndexToEffectInstance.ContainsKey(eliteBuffIndex))
            {
                var parent = characterModel ? characterModel.transform : body ? body.transform : transform;
                var effect = Instantiate(eed.effect, parent, false);
                _eliteBuffIndexToEffectInstance[eliteBuffIndex] = effect;
            }
            if (!_eliteBuffIndexToEffectInstance[eliteBuffIndex]) //It could be that the C++ object was destroyed but not the C# one, instantiate it again just in case.
            {
                var parent = characterModel ? characterModel.transform : body ? body.transform : transform;
                var effect = Instantiate(eed.effect, parent, false);
                _eliteBuffIndexToEffectInstance[eliteBuffIndex] = effect;
            }
            _eliteBuffIndexToEffectInstance[eliteBuffIndex].SetActive(true);
        }

        internal void OnEliteBuffFinalStackLost(BuffIndex eliteBuffIndex, ExtendedEliteDef eed)
        {
            if (!_eliteBuffIndexToEffectInstance.ContainsKey(eliteBuffIndex))
                return;

            if (!_eliteBuffIndexToEffectInstance[eliteBuffIndex]) //It could be that the C++ object was destroyed but not the C# one, instantiate it again just in case.
                return;
                
            _eliteBuffIndexToEffectInstance[eliteBuffIndex].SetActive(false);
        }
    }
}
