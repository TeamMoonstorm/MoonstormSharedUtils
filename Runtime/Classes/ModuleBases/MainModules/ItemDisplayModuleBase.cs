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
        internal static readonly Dictionary<string, GameObject> moonstormItemDisplayPrefabs = new Dictionary<string, GameObject>();

        internal static readonly Dictionary<string, ItemDisplayRuleSet> vanillaIDRS = new Dictionary<string, ItemDisplayRuleSet>();

        internal static readonly Dictionary<string, Object> itemKeyAssets = new Dictionary<string, Object>();

        internal static readonly Dictionary<string, Object> equipKeyAssets = new Dictionary<string, Object>();

        internal static readonly List<ItemDisplayDictionary> singleItemDisplayRules = new List<ItemDisplayDictionary>();

        internal static readonly List<NamedIDRS> moonstormItemDisplayRuleSets = new List<NamedIDRS>();


        [SystemInitializer(typeof(PickupCatalog), typeof(BodyCatalog))]
        private static void HookInit()
        {
            MSULog.Info("Subscribing to delegates related to ItemDisplays.");

            PopulateFromBody("Commando");
            PopulateFromBody("Croco");
            PopulateFromBody("Mage");
            PopulateFromBody("LunarExploder");

            GetAllVanillaIDRS();

            typeof(RoR2Content.Items)
                .GetFields()
                .ToList()
                .ForEach(Field => itemKeyAssets.Add(Field.Name.ToLowerInvariant(), Field.GetValue(typeof(ItemDef)) as Object));

            //this is stupid.
            itemKeyAssets.Remove("burnnearby");

            typeof(RoR2Content.Equipment)
                .GetFields()
                .ToList()
                .ForEach(field => equipKeyAssets.Add(field.Name.ToLowerInvariant(), field.GetValue(typeof(EquipmentDef)) as Object));

            RoR2Application.onLoad += FinishIDRS;
        }
        public override void Initialize()
        {
        }

        #region Methods
        [Obsolete("Please remove it as it is no longer used")]
        public void PopulateVanillaIDRSFromAssetBundle()
        {
            MSULog.Info($"The method of name {nameof(PopulateVanillaIDRSFromAssetBundle)} is deprecated, please remove it as it is no longer used.\nCalled From {Assembly.GetCallingAssembly().GetName().Name}");
        }

        /// <summary>
        /// Populates all the Key assets and Item Display Prefabs from your Assetbundle
        /// </summary>
        public void PopulateKeyAssetsAndDisplaysFromAssetbundle()
        {
            MainBundle.LoadAllAssets<KeyAssetDisplayPairHolder>()
                .ToList()
                .ForEach(so =>
                {
                    so.KeyAssetDisplayPairs
                    .ToList()
                    .ForEach(kadp => kadp.AddKeyAssetDisplayPrefabsToIDRS());
                });
        }

        /// <summary>
        /// Populates all your MSIDRS from your Assetbundle
        /// </summary>
        public void PopulateMSIDRSFromAssetBundle()
        {
            MainBundle.LoadAllAssets<NamedIDRS>()
                .ToList()
                .ForEach(idrs =>
                {
                    if (!moonstormItemDisplayRuleSets.Contains(idrs))
                        moonstormItemDisplayRuleSets.Add(idrs);
                });
        }

        /// <summary>
        /// Populates all your SingleItemDisplayRules from your Assetbundle
        /// </summary>
        public void PopulateSingleItemDisplayRuleFromAssetBundle()
        {
            MainBundle.LoadAllAssets<ItemDisplayDictionary>()
                .ToList()
                .ForEach(sidrs =>
                {
                    if (!singleItemDisplayRules.Contains(sidrs))
                        singleItemDisplayRules.Add(sidrs);
                });
        }
        #endregion

        private static void FinishIDRS()
        {
            /*if (MSUConfig.EnableLoggingOfIDRS.Value)
                LogEverything();

            MSULog.Info("Finishing IDRS");
            foreach (NamedIDRS idrs in moonstormItemDisplayRuleSets)
            {
                idrs.FetchIDRS();
                if (idrs.vanillaIDRS)
                {
                    idrs.GetItemDisplayRules()
                        .ToList()
                        .ForEach(itemDisplayRule =>
                        {
                            HG.ArrayUtils.ArrayAppend(ref idrs.vanillaIDRS.keyAssetRuleGroups, itemDisplayRule);
                        });
                    idrs.vanillaIDRS.GenerateRuntimeValues();
                }
                MSULog.Debug($"Finished appending values in {idrs}");
            }
            for (int i = 0; i < singleItemDisplayRules.Count; i++)
            {
                var currentI = singleItemDisplayRules[i];
                for (int j = 0; j < currentI.singleItemDisplayRules.Count; j++)
                {
                    var currentJ = currentI.singleItemDisplayRules[j];
                    currentJ.FetchIDRS();
                    if (!currentJ.vanillaIDRS)
                    {
                        MSULog.Debug($"Could not find IDRS or name {currentJ.vanillaIDRSKey} in the dictionary, skipping.");
                        continue;
                    }
                    for (int k = 0; k < currentJ.itemDisplayRules.Count; k++)
                    {
                        var currentK = currentJ.itemDisplayRules[k];
                        var toAppend = currentI.Parse(j);
                        if (toAppend.keyAsset != null)
                        {
                            HG.ArrayUtils.ArrayAppend(ref currentJ.vanillaIDRS.keyAssetRuleGroups, toAppend);
                        }
                    }
                    currentJ.vanillaIDRS.GenerateRuntimeValues();
                }
                MSULog.Debug($"Finished appending values in {currentI}");
            }

            //Clears the enumerables because they're no longer needed.
            moonstormItemDisplayPrefabs.Clear();
            vanillaIDRS.Clear();
            itemKeyAssets.Clear();
            equipKeyAssets.Clear();
            singleItemDisplayRules.Clear();
            moonstormItemDisplayRuleSets.Clear();

            MSULog.Debug("Cleared up memory by clearing static enumerables.");*/
        }

        private static void LogEverything()
        {
            int amount = 0;
            List<string> toLog = new List<string>();
            toLog.Add("(These keys are case insensitive.)");
            toLog.Add("Loaded Item Display Prefabs\n---------------------------");
            foreach (var kvp in moonstormItemDisplayPrefabs)
            {
                toLog.Add(kvp.Key);
                amount++;
            }
            toLog.Add("Loaded Item Key Assets\n---------------------------");
            foreach (var kvp in itemKeyAssets)
            {
                toLog.Add(kvp.Key);
                amount++;
            }

            toLog.Add("Loaded Equipment Key Assets\n---------------------------");
            foreach (var kvp in equipKeyAssets)
            {
                toLog.Add(kvp.Key);
                amount++;
            }

            toLog.Add("Loaded Vanilla IDRS\n---------------------------");
            foreach (var kvp in vanillaIDRS)
            {
                toLog.Add(kvp.Key);
                amount++;
            }

            MSULog.Debug(string.Join("\n", toLog));

            MSULog.Info($"Finished logging a total of {amount} related keys for the IDRS system.");
        }

        private static void GetAllVanillaIDRS()
        {
            BodyCatalog.allBodyPrefabs
                .ToList()
                .ForEach(prefab =>
                {
                    var modelLocator = prefab.GetComponent<ModelLocator>();
                    if (modelLocator)
                    {
                        var modelPrefab = modelLocator.modelTransform.gameObject;
                        if (modelPrefab)
                        {
                            var charModel = modelPrefab.GetComponent<CharacterModel>();
                            if (charModel)
                            {
                                var idrs = charModel.itemDisplayRuleSet;
                                if (idrs)
                                {
                                    string key = idrs.name;
                                    if (string.IsNullOrWhiteSpace(idrs.name) || string.IsNullOrEmpty(idrs.name))
                                    {
                                        key = $"idrs{prefab.name.Replace("Body", string.Empty)}";
                                    }
                                    bool flag = vanillaIDRS.ContainsKey(key.ToLowerInvariant());
                                    if (!flag)
                                    {
                                        vanillaIDRS.Add(key.ToLowerInvariant(), idrs);
                                    }
                                }
                            }
                        }
                    }
                });
        }

        private static async void PopulateFromBody(string bodyName)
        {
            var asyncOp = Addressables.LoadAssetAsync<ItemDisplayRuleSet>($"RoR2/Base/{bodyName}/idrs{bodyName}.asset");

            ItemDisplayRuleSet itemDisplayRuleSet = await asyncOp.Task;

            ItemDisplayRuleSet.KeyAssetRuleGroup[] item = itemDisplayRuleSet.keyAssetRuleGroups;

            for (int i = 0; i < item.Length; i++)
            {
                ItemDisplayRule[] rules = item[i].displayRuleGroup.rules;

                for (int j = 0; j < rules.Length; j++)
                {
                    GameObject followerPrefab = rules[j].followerPrefab;
                    if (followerPrefab)
                    {
                        string name = followerPrefab.name;
                        string key = name?.ToLowerInvariant();
                        if (!moonstormItemDisplayPrefabs.ContainsKey(key))
                        {
                            moonstormItemDisplayPrefabs[key] = followerPrefab;
                        }
                    }
                }
            }
        }
        internal static GameObject LoadDisplay(string name)
        {
            if (moonstormItemDisplayPrefabs.ContainsKey(name.ToLowerInvariant()))
            {
                if (moonstormItemDisplayPrefabs[name.ToLowerInvariant()]) return moonstormItemDisplayPrefabs[name.ToLowerInvariant()];
            }
            return null;
        }
    }
}
