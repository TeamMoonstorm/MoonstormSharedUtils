using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.ObjectModel;

namespace MSU
{
    [CreateAssetMenu(fileName = "New InteractableCardProvider", menuName = "MSU/DirectorCardProviders/InteractableCardProvider")]
    public class InteractableCardProvider : ScriptableObject
    {
        public ReadOnlyDictionary<DirectorAPI.Stage, DirectorAPI.DirectorCardHolder> StageToCards
        {
            get
            {
                if (_stageToCards == null)
                {
                    var dict = new Dictionary<DirectorAPI.Stage, DirectorAPI.DirectorCardHolder>();
                    foreach (var stageInteractableCardPair in _serializedCardPairs)
                    {
                        if (stageInteractableCardPair.stageEnum == DirectorAPI.Stage.Custom)
                            continue;

                        if (dict.ContainsKey(stageInteractableCardPair.stageEnum))
                            continue;

                        dict.Add(stageInteractableCardPair.stageEnum, stageInteractableCardPair);
                    }
                    _stageToCards = new ReadOnlyDictionary<DirectorAPI.Stage, DirectorAPI.DirectorCardHolder>(dict);
                }
                return _stageToCards;
            }
        }
        private ReadOnlyDictionary<DirectorAPI.Stage, DirectorAPI.DirectorCardHolder> _stageToCards;

        public ReadOnlyDictionary<string, DirectorAPI.DirectorCardHolder> CustomStageToCards
        {
            get
            {
                if (_stageToCards == null)
                {
                    var dict = new Dictionary<string, DirectorAPI.DirectorCardHolder>();
                    foreach (var stageInteractableCardPair in _serializedCardPairs)
                    {
                        if (stageInteractableCardPair.stageEnum != DirectorAPI.Stage.Custom)
                            continue;

                        if (dict.ContainsKey(stageInteractableCardPair.customStageName))
                            continue;

                        dict.Add(stageInteractableCardPair.customCategoryName, stageInteractableCardPair);
                    }

                    _customStageToCards = new ReadOnlyDictionary<string, DirectorAPI.DirectorCardHolder>(dict);
                }
                return _customStageToCards;
            }
        }
        private ReadOnlyDictionary<string, DirectorAPI.DirectorCardHolder> _customStageToCards;

        [SerializeField]
        private StageInteractableCardPair[] _serializedCardPairs = Array.Empty<StageInteractableCardPair>();

        [Serializable]
        public struct StageInteractableCardPair
        {
            public DirectorAPI.Stage stageEnum;
            public string customStageName;
            public DirectorAPI.InteractableCategory interactableCategory;
            public float customCategoryWeight;
            public string customCategoryName;

            public AddressableDirectorCard card;

            public static implicit operator DirectorAPI.DirectorCardHolder(StageInteractableCardPair other)
            {
                return new DirectorAPI.DirectorCardHolder
                {
                    Card = new DirectorCard
                    {
                        forbiddenUnlockableDef = other.card.forbiddenUnlockableDef,
                        minimumStageCompletions = other.card.minimumStageCompletions,
                        preventOverhead = other.card.preventOverhead,
                        requiredUnlockableDef = other.card.requiredUnlockableDef,
                        selectionWeight = other.card.selectionWeight,
                        spawnCard = other.card.spawnCard,
                        spawnDistance = other.card.spawnDistance
                    },
                    InteractableCategory = other.interactableCategory,
                    CustomInteractableCategory = other.interactableCategory == DirectorAPI.InteractableCategory.Custom ? other.customCategoryName : null,
                    InteractableCategorySelectionWeight = other.interactableCategory == DirectorAPI.InteractableCategory.Custom ? other.customCategoryWeight : 0,

                    MonsterCategorySelectionWeight = 0,
                    CustomMonsterCategory = null,
                    MonsterCategory = DirectorAPI.MonsterCategory.Invalid
                };
            }
        }
    }
}
