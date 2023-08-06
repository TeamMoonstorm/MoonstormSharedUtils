using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete("Use R2API's AddressableDirectorCard instead")]
    [Serializable]
    public class MSDirectorCard
    {
        public AddressableSpawnCard spawnCard;
        public int selectionWeight;
        public DirectorCore.MonsterSpawnDistance spawnDistance;
        public bool preventOverhead;
        public int minimumStageCompletions;
        public AddressableUnlockableDef requiredUnlockableDef;
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
    [Obsolete("Use R2API's AddressableDirectorCardCategorySelection instead")]
    public class MSDirectorCardCategorySelection : ScriptableObject
    {
        [Serializable]
        public struct Category
        {
            public string name;
            public MSDirectorCard[] cards;
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
        public DirectorCardCategorySelection targetCardCategorySelection;
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
