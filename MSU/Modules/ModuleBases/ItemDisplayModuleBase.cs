using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for managing Item Displays
    /// </summary>
    public abstract class ItemDisplayModuleBase : ModuleBase
    {
        private static bool populatedDictionary = false;

        internal static readonly Dictionary<string, GameObject> moonstormItemDisplayPrefabs = new Dictionary<string, GameObject>();

        internal static readonly Dictionary<string, ItemDisplayRuleSet> vanillaIDRS = new Dictionary<string, ItemDisplayRuleSet>();

        internal static readonly Dictionary<string, Object> itemKeyAssets = new Dictionary<string, Object>();

        internal static readonly Dictionary<string, Object> equipKeyAssets = new Dictionary<string, Object>();

        internal static readonly List<MSSingleItemDisplayRule> singleItemDisplayRules = new List<MSSingleItemDisplayRule>();

        internal static readonly List<MSIDRS> moonstormItemDisplayRuleSets = new List<MSIDRS>();


        [SystemInitializer(typeof(PickupCatalog))]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to ItemDisplays.");
            RoR2Application.onLoad += FinishIDRS;
        }

        /// <summary>
        /// Initialize your ItemDisplays
        /// <para>calling base.Init() REQUIRED</para>
        /// </summary>
        public override void Init()
        {
            //Gotta populate the dictionary once
            if (!populatedDictionary)
            {
                populatedDictionary = true;

                PopulateFromBody("Commando");
                PopulateFromBody("LunarExploder");

                PopulateVanillaIDRS();

                typeof(RoR2Content.Items)
                    .GetFields()
                    .ToList()
                    .ForEach(Field => itemKeyAssets.Add(Field.Name.ToLowerInvariant(), Field.GetValue(typeof(ItemDef)) as Object));

                typeof(RoR2Content.Equipment)
                    .GetFields()
                    .ToList()
                    .ForEach(field => equipKeyAssets.Add(field.Name.ToLowerInvariant(), field.GetValue(typeof(EquipmentDef)) as Object));
            }
        }

        #region Methods
        /// <summary>
        /// Populates all your IDRS found in your Assetbundle.
        /// </summary>
        public void PopulateVanillaIDRSFromAssetBundle()
        {
            AssetBundle.LoadAllAssets<ItemDisplayRuleSet>()
                       .ToList()
                       .ForEach(IDRS =>
                       {
                           bool flag = vanillaIDRS.ContainsKey(IDRS.name.ToLowerInvariant());
                           if (!flag)
                               vanillaIDRS.Add(IDRS.name.ToLowerInvariant(), IDRS);
                       });
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

            MSULog.LogI("Finishing IDRS");
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
            }
            for (int i = 0; i < singleItemDisplayRules.Count; i++)
            {
                var currentI = singleItemDisplayRules[i];
                for (int j = 0; j < currentI.SingleItemDisplayRules.Count; j++)
                {
                    var currentJ = currentI.SingleItemDisplayRules[j];
                    currentJ.FetchIDRS();
                    for (int k = 0; k < currentJ.ItemDisplayRules.Count; k++)
                    {
                        var currentK = currentJ.ItemDisplayRules[k];
                        var toAppend = currentI.Parse(j);
                        if (toAppend.keyAsset != null)
                        {
                            HG.ArrayUtils.ArrayAppend(ref currentJ.vanillaIDRS.keyAssetRuleGroups, toAppend);
                        }
                    }
                    currentJ.vanillaIDRS.GenerateRuntimeValues();
                }
            }

            //Clears the enumerables because they're no longer needed.
            moonstormItemDisplayPrefabs.Clear();
            vanillaIDRS.Clear();
            itemKeyAssets.Clear();
            equipKeyAssets.Clear();
            singleItemDisplayRules.Clear();
            moonstormItemDisplayRuleSets.Clear();

            MSULog.LogD("Cleared up memory by clearing static enumerables.");
        }

        private static void LogEverything()
        {
            List<string> toLog = new List<string>();
            toLog.Add("Loaded Item Display Prefabs\n---------------------------");
            foreach (var kvp in moonstormItemDisplayPrefabs)
                toLog.Add(kvp.Key);

            toLog.Add("Loaded Item Key Assets\n---------------------------");
            foreach (var kvp in itemKeyAssets)
                toLog.Add(kvp.Key);

            toLog.Add("Loaded Equipment Key Assets\n---------------------------");
            foreach (var kvp in equipKeyAssets)
                toLog.Add(kvp.Key);

            toLog.Add("Loaded Vanilla IDRS\n---------------------------");
            foreach (var kvp in vanillaIDRS)
                toLog.Add(kvp.Key);

            MSULog.LogD(string.Join("\n", toLog));
        }

        private static void PopulateVanillaIDRS()
        {
            List<ItemDisplayRuleSet> IDRSList = new List<ItemDisplayRuleSet>();
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
