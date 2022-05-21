using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    public abstract class ItemDisplayModuleBase : BundleModule
    {
        private static List<NamedIDRS> namedIdrss = new List<NamedIDRS>();
        private static List<ItemDisplayDictionary> itemDisplayDictionaries = new List<ItemDisplayDictionary>();

        [SystemInitializer(typeof(PickupCatalog), typeof(BodyCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing ItemDisplays Module...");
            AddressableAssets.AddressableAsset.OnAddressableAssetsLoaded += FinishIDRS;
        }

        #region Methods
        protected void AddNamedIDRSFromMainBundle()
        {
            namedIdrss.AddRange(LoadAll<NamedIDRS>());
        }

        protected void AddItemDisplayDictionariesFromMainBundle()
        {
            itemDisplayDictionaries.AddRange(LoadAll<ItemDisplayDictionary>());
        }
        #endregion

        #region IDRS Finish
        private static void FinishIDRS()
        {
            MSULog.Info($"Finishing IDRS");
            foreach(NamedIDRS namedIdrs in namedIdrss)
            {
                foreach(ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup in namedIdrs.GetKeyAssetRuleGroups())
                {
                    HG.ArrayUtils.ArrayAppend(ref namedIdrs.idrs.keyAssetRuleGroups, keyAssetRuleGroup);
                }
                namedIdrs.idrs.GenerateRuntimeValues();
                MSULog.Debug($"Finished appending values from {namedIdrs} to {namedIdrs.idrs}");
            }

            foreach(ItemDisplayDictionary itemDisplayDictionary in itemDisplayDictionaries)
            {
                for(int i = 0; i < itemDisplayDictionary.namedDisplayDictionary.Count; i++)
                {
                    var current = itemDisplayDictionary.namedDisplayDictionary[i];
                    var keyAssetRuleGroup = itemDisplayDictionary.GetKeyAssetRuleGroup(current.idrs.Asset);
                    HG.ArrayUtils.ArrayAppend(ref current.idrs.Asset.keyAssetRuleGroups, keyAssetRuleGroup);

                    MSULog.Debug($"Finished appending values from {itemDisplayDictionary}'s {i} entry into {current.idrs.Asset}");
                }
                MSULog.Debug($"Finished appending values of {itemDisplayDictionary}");
            }

            MSULog.Debug($"IDRS Setup finished, clearing static enumerables.");
            itemDisplayDictionaries = null;
            namedIdrss = null;
        }
        #endregion
    }
}
