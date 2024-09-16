using MSU;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        internal void AssignNewElite(EliteIndex eliteIndex)
        {
            //Incoming index is none, or the incoming index is not an ExtendedEliteDef, destroy effect instance if needed.
            if (eliteIndex == EliteIndex.None || !(EliteCatalog.GetEliteDef(eliteIndex) is ExtendedEliteDef eed))
            {
                if (_effectInstance)
                    Destroy(_effectInstance);
                return;
            }

            if (!eed || !eed.effect)
                return;

            _effectInstance = Instantiate(eed.effect, body.aimOriginTransform, false);

        }
    }
}
