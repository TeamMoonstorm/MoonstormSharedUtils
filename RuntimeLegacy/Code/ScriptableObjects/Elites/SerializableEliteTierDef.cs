using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using static RoR2.CombatDirector;

namespace Moonstorm
{
    public class SerializableEliteTierDef : ScriptableObject
    {
        public float costMultiplier;
        public AddressReferencedEliteDef[] elites = Array.Empty<AddressReferencedEliteDef>();
        public bool canSelectWithoutAvailableEliteDef;

        public EliteTierDef EliteTierDef { get; private set; }
        
        internal void Create()
        {
            EliteTierDef = new EliteTierDef
            {
                eliteTypes = elites.Select(x => x.Asset).ToArray(),
                costMultiplier = costMultiplier,
                canSelectWithoutAvailableEliteDef = canSelectWithoutAvailableEliteDef,
                isAvailable = (rules) => true,
            };
        }
    }
}
