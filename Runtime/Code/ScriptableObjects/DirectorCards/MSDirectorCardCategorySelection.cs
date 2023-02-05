using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// Represents a <see cref="DirectorCard"/> that can be created from using AddressableAssets, specifically <see cref="AddressableSpawnCard"/> and <see cref="AddressableUnlockableDef"/>
    /// </summary>
    [Serializable]
    public class MSDirectorCard
    {
        [Tooltip("The spawn card for this DirectorCard")]
        public AddressableSpawnCard spawnCard;
        [Tooltip("The weight of this director card relative to other cards")]
        public int selectionWeight;
        [Tooltip("The distance used for spawning this card, used for monsters")]
        public DirectorCore.MonsterSpawnDistance spawnDistance;
        public bool preventOverhead;
        [Tooltip("The minimum amount of stages that need to be completed before this Card can be spawned")]
        public int minimumStageCompletions;
        [Tooltip("This unlockableDef must be unlocked for this Card to spawn")]
        public AddressableUnlockableDef requiredUnlockableDef;
        [Tooltip("This unlockableDef cannot be unlocked for this Card to spawn")]
        public AddressableUnlockableDef forbiddenUnlockableDef;
        internal DirectorCard Upgrade()
        {
            DirectorCard returnVal = new DirectorCard();
            returnVal.spawnCard = spawnCard;
            returnVal.selectionWeight = selectionWeight;
            returnVal.spawnDistance = spawnDistance;
            returnVal.preventOverhead = preventOverhead;
            returnVal.minimumStageCompletions = minimumStageCompletions;
            returnVal.requiredUnlockableDef = requiredUnlockableDef;
            returnVal.forbiddenUnlockableDef = forbiddenUnlockableDef;
            return returnVal;
        }
    }
    /// <summary>
    /// A <see cref="MSDirectorCardCategorySelection"/> is a version of a <see cref="DirectorCardCategorySelection"/> that can be used for creating a custom, and complex DirectorCardCategorySelection for stages, using addressables to load and spawn vanilla enemies for custom stages.
    /// <para>All the values from this category selection will be added to the <see cref="DirectorCardCategorySelection"/> specified in <see cref="targetCardCategorySelection"/></para>
    /// <para>You should also see <see cref="MSDCCSPool"/></para>
    /// </summary>
    [CreateAssetMenu(fileName = "New MSDirectorCardCategorySelection", menuName = "Moonstorm/Director Cards/MSDirectorCardCategorySelection")]
    public class MSDirectorCardCategorySelection : ScriptableObject
    {
        /// <summary>
        /// Represents a category of spawn cards
        /// </summary>
        [Serializable]
        public struct Category
        {
            [Tooltip("The name of this category")]
            public string name;
            [Tooltip("The DirectorCards for this category")]
            public MSDirectorCard[] cards;
            [Tooltip("The weight of this category relative to the other categories")]
            public float selectionWeight;

            internal DirectorCardCategorySelection.Category Upgrade()
            {
                DirectorCardCategorySelection.Category returnVal = default(DirectorCardCategorySelection.Category);
                returnVal.name = name;
                returnVal.cards = cards.Select(x => x.Upgrade()).ToArray();
                returnVal.selectionWeight = selectionWeight;
                return returnVal;
            }
        }
        /// <summary>
        /// The target <see cref="DirectorCardCategorySelection"/> that will be overriden with the values stored in this MSDirectorCardCategorySelection
        /// </summary>
        [Tooltip("The DirectorCardCategorySelection that will be overriden with the values stored in this MSDirectorCardCategorySelection")]
        public DirectorCardCategorySelection targetCardCategorySelection;
        [Tooltip("The categories for this MSDirectorCardCategorySelection")]
        public Category[] categories = Array.Empty<Category>();
        internal void Upgrade()
        {
            targetCardCategorySelection.categories = categories.Select(x => x.Upgrade()).ToArray();
            categories = null;
        }
        private void Awake() => instances.AddIfNotInCollection(this);
        private void Destroy() => instances.RemoveIfInCollection(this);

        private static readonly List<MSDirectorCardCategorySelection> instances = new List<MSDirectorCardCategorySelection>();
        [SystemInitializer]
        private static void SystemInitializer()
        {
            AddressableAsset.OnAddressableAssetsLoaded += () =>
            {
                MSULog.Info("Initializing MSDirectorCardCategorySelection");
                foreach (MSDirectorCardCategorySelection selection in instances)
                {
                    selection.Upgrade();
                }
            };
        }
    }
}
