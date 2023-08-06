using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete("Use R2API's AddressableDCCSPool instead")]
    public class MSDCCSPool : ScriptableObject
    {
        public DccsPool targetPool;

        [Serializable]
        public class MSPoolEntry
        {
            public MSDirectorCardCategorySelection dccs;
            public float weight;

            internal virtual DccsPool.PoolEntry Upgrade()
            {
                DccsPool.PoolEntry returnValue = new DccsPool.PoolEntry();
                returnValue.dccs = dccs.targetCardCategorySelection;
                returnValue.weight = weight;
                return returnValue;
            }
        }

        [Serializable]
        public class MSCategory
        {
            public string name;
            public float categoryWeight = 1f;
            public MSPoolEntry[] alwaysIncluded = Array.Empty<MSPoolEntry>();
            public MSConditionalPoolEntry[] includedIfConditionsMet = Array.Empty<MSConditionalPoolEntry>();
            public MSPoolEntry[] includedIfNoConditionsMet = Array.Empty<MSPoolEntry>();

            internal DccsPool.Category Upgrade()
            {
                DccsPool.Category returnValue = new DccsPool.Category();
                returnValue.name = name;
                returnValue.categoryWeight = categoryWeight;
                returnValue.alwaysIncluded = alwaysIncluded.Select(x => x.Upgrade()).ToArray();
                returnValue.includedIfConditionsMet = includedIfConditionsMet.Select(y => y.Upgrade()).Cast<DccsPool.ConditionalPoolEntry>().ToArray();
                returnValue.includedIfNoConditionsMet = includedIfNoConditionsMet.Select(z => z.Upgrade()).ToArray();
                return returnValue;
            }
        }
        [Serializable]
        public class MSConditionalPoolEntry : MSPoolEntry
        {
            public AddressableExpansionDef[] requiredExpansions = Array.Empty<AddressableExpansionDef>();

            internal override DccsPool.PoolEntry Upgrade()
            {
                DccsPool.ConditionalPoolEntry returnValue = new DccsPool.ConditionalPoolEntry();
                returnValue.weight = weight;
                returnValue.dccs = dccs.targetCardCategorySelection;
                returnValue.requiredExpansions = requiredExpansions.Select(x => x.Asset).ToArray();
                return returnValue;
            }
        }

        public MSCategory[] poolCategories = Array.Empty<MSCategory>();
        private void Upgrade()
        {
            DccsPool.Category[] categories = poolCategories.Select(x => x.Upgrade()).ToArray();
            poolCategories = null;
        }
        private void Awake() => instances.AddIfNotInCollection(this);
        private void OnDestroy() => instances.RemoveIfInCollection(this);

        private static readonly List<MSDCCSPool> instances = new List<MSDCCSPool>();
        [SystemInitializer]
        private static void SystemInitializer()
        {
            AddressableAsset.OnAddressableAssetsLoaded += () =>
            {
                MSULog.Info("Initializing MSDCCSPool");
                foreach (MSDCCSPool pool in instances)
                {
                    pool.Upgrade();
                }
            };
        }
    }
}