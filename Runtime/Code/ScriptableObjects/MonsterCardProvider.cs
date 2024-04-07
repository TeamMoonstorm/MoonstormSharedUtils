using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    [CreateAssetMenu(fileName = "New MonsterCardProvider", menuName = "MSU/DirectorCardProviders/MonsterCardProvider")]
    public class MonsterCardProvider : ScriptableObject
    {
        public ReadOnlyDictionary<DirectorAPI.Stage, DirectorAPI.DirectorCardHolder> StageToCards
        {
            get
            {
                if(_stageToCards == null)
                {
                    var dict = new Dictionary<DirectorAPI.Stage, DirectorAPI.DirectorCardHolder>();
                    foreach(var stageMonsterCardPair in _serializedCardPairs)
                    {
                        if (stageMonsterCardPair.stageEnum == DirectorAPI.Stage.Custom)
                            continue;

                        if (dict.ContainsKey(stageMonsterCardPair.stageEnum))
                            continue;

                        dict.Add(stageMonsterCardPair.stageEnum, stageMonsterCardPair);
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
                if(_stageToCards == null)
                {
                    var dict = new Dictionary<string, DirectorAPI.DirectorCardHolder>();
                    foreach(var stageMonsterCardPair in _serializedCardPairs)
                    {
                        if (stageMonsterCardPair.stageEnum != DirectorAPI.Stage.Custom)
                            continue;

                        if (dict.ContainsKey(stageMonsterCardPair.customStageName))
                            continue;

                        dict.Add(stageMonsterCardPair.customCategoryName, stageMonsterCardPair);
                    }

                    _customStageToCards = new ReadOnlyDictionary<string, DirectorAPI.DirectorCardHolder>(dict);
                }
                return _customStageToCards;
            }
        }
        private ReadOnlyDictionary<string, DirectorAPI.DirectorCardHolder> _customStageToCards;

        [SerializeField]
        private StageMonsterCardPair[] _serializedCardPairs = Array.Empty<StageMonsterCardPair>();

        [Serializable]
        public struct StageMonsterCardPair
        {
            public DirectorAPI.Stage stageEnum;
            public string customStageName;
            public DirectorAPI.MonsterCategory monsterCategory;
            public float customCategoryWeight;
            public string customCategoryName;

            public AddressableDirectorCard card;

            public static implicit operator DirectorAPI.DirectorCardHolder(StageMonsterCardPair other)
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
                    MonsterCategory = other.monsterCategory,
                    CustomMonsterCategory = other.monsterCategory == DirectorAPI.MonsterCategory.Custom ? other.customCategoryName : null,
                    MonsterCategorySelectionWeight = other.monsterCategory == DirectorAPI.MonsterCategory.Custom ? other.customCategoryWeight : 0,

                    InteractableCategorySelectionWeight = 0,
                    CustomInteractableCategory = null,
                    InteractableCategory = DirectorAPI.InteractableCategory.Invalid
                };
            }
        }
    }
}
