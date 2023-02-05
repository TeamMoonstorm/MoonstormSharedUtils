using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="MSDCCSPool"/> is a version of a <see cref="DccsPool"/> that can be created from the editor itself, it allows you to create complex DccsPools using Addressables and your own existing spawn cards.
    /// <para>All the values from this pool will be added to the <see cref="DccsPool"/> specified in <see cref="targetPool"/></para>
    /// <para>You should also see <see cref="MSDirectorCardCategorySelection"/></para>
    /// </summary>
    [CreateAssetMenu(fileName = "New MSDCCSPool", menuName = "Moonstorm/Director Cards/MSDCCSPool")]
    public class MSDCCSPool : ScriptableObject
    {
        [Tooltip("The DccsPool that will be overriden with the values stored in this MSDCCSPool")]
        public DccsPool targetPool;

        /// <summary>
        /// Represents a version of <see cref="DccsPool.PoolEntry"/> that uses <see cref="MSDirectorCardCategorySelection"/> for representing a pool entry
        /// </summary>
        [Serializable]
        public class MSPoolEntry
        {
            [Tooltip("The DCCS for this pool entry")]
            public MSDirectorCardCategorySelection dccs;
            [Tooltip("The weight of this pool entry relative to the others")]
            public float weight;

            internal virtual DccsPool.PoolEntry Upgrade()
            {
                DccsPool.PoolEntry returnValue = new DccsPool.PoolEntry();
                returnValue.dccs = dccs.targetCardCategorySelection;
                returnValue.weight = weight;
                return returnValue;
            }
        }
        /// <summary>
        /// Represents a category of DirectorCardCategorySelections for this pool
        /// </summary>
        [Serializable]
        public class MSCategory
        {
            [Tooltip("A name to help identify this category")]
            public string name;
            [Tooltip("The weight of all entries in this category relative to the sibling categories.")]
            public float categoryWeight = 1f;
            [Tooltip("These entries are always considered.")]
            public MSPoolEntry[] alwaysIncluded = Array.Empty<MSPoolEntry>();
            [Tooltip("These entries are only considered if their individual conditions are met.")]
            public MSConditionalPoolEntry[] includedIfConditionsMet = Array.Empty<MSConditionalPoolEntry>();
            [Tooltip("These entries are considered only if no entries from 'includedIfConditionsMet' have been included.")]
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
        /// <summary>
        /// Represents a conditional version of a <see cref="MSPoolEntry"/>
        /// <para>Contains a <see cref="requiredExpansions"/> array that's populated using <see cref="AddressableExpansionDef"/>s</para>
        /// </summary>
        [Serializable]
        public class MSConditionalPoolEntry : MSPoolEntry
        {
            [Tooltip("ALL expansions in this list must be enabled for this run for this entry to be considered.")]
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

        [Tooltip("The categories for this pool")]
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