using R2API;
using R2API.AddressReferencedAssets;
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
    /// Represents a Serialized version of a <see cref="CombatDirector.EliteTierDef"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New SerializableEliteTierDef", menuName = "MSU/SerializableEliteTierDef")]
    public class SerializableEliteTierDef : ScriptableObject
    {
        [Tooltip("The cost multiplier applied to a monster's cost when it tries to spawn as an elite from this tier.")]
        public float costMultiplier;
        [Tooltip("The elites that are contained in this tier")]
        public AddressReferencedEliteDef[] elites = Array.Empty<AddressReferencedEliteDef>();
        [Tooltip("Checks wether the tier can be selected even if there are no available elites")]
        public bool canBeSelectedWithoutAvailableEliteDef;

        /// <summary>
        /// Stores the finalized EliteTierDef, Null by default and gets populated by <see cref="Init"/>
        /// </summary>
        public CombatDirector.EliteTierDef EliteTierDef { get; private set; }

        /// <summary>
        /// Stores the EliteTierIndex that was assigned when <see cref="EliteTierDef"/> was populated
        /// </summary>
        public int EliteTierIndex { get; private set; }

        /// <summary>
        /// Checks wether or not this EliteDef is available. Override this to implement custom availability checking.
        /// </summary>
        public Func<SpawnCard.EliteRules, bool> IsAvailableCheck { get; set; }

        /// <summary>
        /// Creates an initializes the EliteTierDef
        /// </summary>
        public void Init()
        {
            if(EliteTierDef != null || EliteTierIndex != -1)
            {
                return;
            }

            if(AddressReferencedAsset.Initialized)
            {
                DoInit();
            }
            else
            {
                AddressReferencedAsset.OnAddressReferencedAssetsLoaded += DoInit;
            }
        }

        private void DoInit()
        {
            IsAvailableCheck = IsAvailableCheck ?? DefaultAvailability;

            EliteTierDef = new CombatDirector.EliteTierDef
            {
                eliteTypes = elites.Where(x => x.AssetExists).Select(x => x.Asset).ToArray(),
                canSelectWithoutAvailableEliteDef = canBeSelectedWithoutAvailableEliteDef,
                isAvailable = IsAvailableCheck,
                costMultiplier = costMultiplier,
            };
            EliteTierIndex = EliteAPI.AddCustomEliteTier(EliteTierDef);
        }

        private static bool DefaultAvailability(SpawnCard.EliteRules rules) => true;
    }
}