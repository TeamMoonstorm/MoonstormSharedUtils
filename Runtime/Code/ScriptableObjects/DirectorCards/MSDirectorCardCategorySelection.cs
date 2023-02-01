using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using Moonstorm.AddressableAssets;

namespace Moonstorm
{
    [Serializable]
    internal class MSDirectorCard
    {
        public AddressableSpawnCard spawnCard;
        public int selectionWeight;
        public DirectorCore.MonsterSpawnDistance spawnDistance;
        public bool preventOverhead;
        public int minimumStageCompletions;
        public AddressableUnlockableDef requiredUnlockableDef;
        public AddressableUnlockableDef forbiddenUnlockableDef;
        public DirectorCard Upgrade()
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
    [CreateAssetMenu(fileName = "New MSDirectorCardCategorySelection", menuName = "Moonstorm/Director Cards/MSDirectorCardCategorySelection")]
    public class MSDirectorCardCategorySelection : ScriptableObject
    {
        [Serializable]
        public struct Category
        {
            public string name;
            [SerializeField]
            internal MSDirectorCard[] cards;
            public float selectionWeight;

            public DirectorCardCategorySelection.Category Upgrade()
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
        protected virtual void Upgrade()
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
                foreach(MSDirectorCardCategorySelection selection in instances)
                {
                    selection.Upgrade();
                }
            };
        }
    }
}
