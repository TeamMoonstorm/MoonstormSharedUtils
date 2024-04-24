using MSU;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.teammoonstorm_moonstormsharedutils.Runtime.Code.Components
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

        private GameObject effectInstance;

        internal void AssignNewElite(EliteIndex eliteIndex)
        {
            //Incoming index is none, or the incoming index is not an ExtendedEliteDef, destroy effect instance if needed.
            if (eliteIndex == EliteIndex.None || !(EliteCatalog.GetEliteDef(eliteIndex) is ExtendedEliteDef eed))
            {
                if (effectInstance)
                    Destroy(effectInstance);
                return;
            }

            if (!eed || !eed.effect)
                return;

            effectInstance = Instantiate(eed.effect, body.aimOriginTransform, false);

        }
    }
}
