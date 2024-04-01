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
    [CreateAssetMenu(fileName = "New SerializableEliteTierDef", menuName = "MSU/SerializableEliteTierDef")]
    public class SerializableEliteTierDef : ScriptableObject
    {
        public float costMultiplier;
        public AddressReferencedEliteDef[] elites = Array.Empty<AddressReferencedEliteDef>();
        public bool canBeSelectedWithoutAvailableEliteDef;

        public CombatDirector.EliteTierDef EliteTierDef { get; private set; }
        public int EliteTierIndex { get; private set; }

        public Func<SpawnCard.EliteRules, bool> IsAvailableCheck { get; private set; }

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