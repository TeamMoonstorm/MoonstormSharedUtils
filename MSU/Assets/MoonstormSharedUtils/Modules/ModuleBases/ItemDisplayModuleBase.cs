using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing Item Displays
    /// <para>Handles proper appending of IDRS values from MSSingleItemDisplayRules and MSIDRS</para>
    /// </summary>
    public abstract class ItemDisplayModuleBase : ModuleBase
    {
        internal static readonly Dictionary<string, GameObject> moonstormItemDisplayPrefabs = new Dictionary<string, GameObject>();

        internal static readonly Dictionary<string, ItemDisplayRuleSet> vanillaIDRS = new Dictionary<string, ItemDisplayRuleSet>();

        internal static readonly Dictionary<string, Object> itemKeyAssets = new Dictionary<string, Object>();

        internal static readonly Dictionary<string, Object> equipKeyAssets = new Dictionary<string, Object>();

        internal static readonly List<MSSingleItemDisplayRule> singleItemDisplayRules = new List<MSSingleItemDisplayRule>();

        internal static readonly List<MSIDRS> moonstormItemDisplayRuleSets = new List<MSIDRS>();


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

        /// <summary>
        /// Initialize your ItemDisplays
        /// </summary>
        public override void Init()
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
            AssetBundle.LoadAllAssets<KeyAssetDisplayPairHolder>()
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
            AssetBundle.LoadAllAssets<MSIDRS>()
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
            AssetBundle.LoadAllAssets<MSSingleItemDisplayRule>()
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
            if (ConfigLoader.EnableLoggingOfIDRS.Value)
                LogEverything();

            MSULog.Info("Finishing IDRS");
            foreach (MSIDRS idrs in moonstormItemDisplayRuleSets)
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

            MSULog.Debug("Cleared up memory by clearing static enumerables.");
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
        private static void PopulateVanillaIDRS()
        {
            Resources.LoadAll<GameObject>("Prefabs/CharacterBodies/")
                .ToList()
                .ForEach(GameObject =>
                {
                    var modelLocator = GameObject.GetComponent<ModelLocator>();
                    if ((bool)modelLocator)
                    {
                        var mdlPrefab = modelLocator.modelTransform.gameObject;
                        if ((bool)mdlPrefab)
                        {
                            var characterModel = mdlPrefab.GetComponent<CharacterModel>();
                            if ((bool)characterModel)
                            {
                                var IDRS = characterModel.itemDisplayRuleSet;
                                if ((bool)IDRS)
                                {
                                    bool flag = vanillaIDRS.ContainsKey(IDRS.name.ToLowerInvariant());
                                    if (!flag)
                                    {
                                        vanillaIDRS.Add(IDRS.name.ToLowerInvariant(), IDRS);
                                    }
                                }
                            }
                        }
                    }
                });
        }

        private static void PopulateFromBody(string bodyName)
        {
            ItemDisplayRuleSet itemDisplayRuleSet = Resources.Load<GameObject>("Prefabs/CharacterBodies/" + bodyName + "Body").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

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
