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
    /// <summary>
    /// A MonsterCardProvider is a ScriptableObject which is used to contain all of a Monster's DirectorCards, which then will be added to the game's stages using <see cref="DirectorAPI"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New MonsterCardProvider", menuName = "MSU/DirectorCardProviders/MonsterCardProvider")]
    public class MonsterCardProvider : ScriptableObject
    {
        /// <summary>
        /// A Dictionary that contains this Monster's <see cref="DirectorAPI.DirectorCardHolder"/> for vanilla stages, which can be accessed by giving the corresponding key of type <see cref="DirectorAPI.Stage"/>
        /// <br>For custom stages, use <see cref="CustomStageToCards"/></br>
        /// </summary>
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

        /// <summary>
        /// A Dictionary that contains this Monster's <see cref="DirectorAPI.DirectorCardHolder"/> for custom stages, which can be accessed by giving the corresponding key, which would be the custom stage's name.
        /// <br>For vanilla stages, use <see cref="StageToCards"/></br>
        /// </summary>
        public ReadOnlyDictionary<string, DirectorAPI.DirectorCardHolder> CustomStageToCards
        {
            get
            {
                if(_customStageToCards == null)
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

        [Tooltip("Contains your Interactable's Cards")]
        [SerializeField]
        private StageMonsterCardPair[] _serializedCardPairs = Array.Empty<StageMonsterCardPair>();

        /// <summary>
        /// Method that builds a <see cref="HashSet{T}"/> containing all the unique instances of SpawnCards held by this MonsterCardProvider. 
        /// <br>Keep in mind that the hash set will not contain SpawnCards obtained from Addressables.</br>
        /// </summary>
        /// <returns>A <see cref="HashSet{T}"/> containing all of the SpawnCards this MonsterCardProvider provides</returns>
        public HashSet<SpawnCard> BuildSpawnCardSet()
        {
            HashSet<SpawnCard> result = new HashSet<SpawnCard>();
            foreach(StageMonsterCardPair pair in _serializedCardPairs)
            {
                var card = pair.card;

                if (card == null)
                    continue;

                var spawnCard = card.spawnCard;
                if(!spawnCard.AssetExists)
                {
                    continue;
                }

                result.Add(spawnCard.Asset);
            }
            return result;
        }

        /// <summary>
        /// Represents a pair of <see cref="DirectorAPI.DirectorCardHolder"/> and stage metadata.
        /// <br>Contains an implicit cast to cast from this struct to <see cref="DirectorAPI.DirectorCardHolder"/></br>
        /// </summary>
        [Serializable]
        public struct StageMonsterCardPair
        {
            [Header("Stage Metadata")]
            [Tooltip("The stage enum for this pair, this only includes vanilla stages.\nif you want to add your interactable to a custom stage, set this to \"Custom\" and fill out the field \"Custom Stage Name\"")]
            public DirectorAPI.Stage stageEnum;

            [Tooltip("The custom stage name for this pair, this is only used for custom, non vanilla stages.\nIf you want to add your interactable to a vanilla stage, leave this empty and utilize the field \"Stage Enum\"")]
            public string customStageName;

            [Header("Director Card Metadata")]
            [Tooltip("The category for this monster. this is only used for vanilla categories. \nIf you want to add your monster to a custom category, set this to \"Custom\" and fill out \"Custom Category Weight\" and \"Custom Category Name\"")]
            public DirectorAPI.MonsterCategory monsterCategory;


            [Tooltip("The weight for a new Category, only relevant if \"Monster Category\" is not \"Custom\".\n keep in mind that if the Category does not exist in the DirectorCardCategorySelection, the category will be added and this weight will be used. if the Category exists, the card will just be added to it.")]
            public float customCategoryWeight;

            [Tooltip("The weight for a new Category, only relevant if \"Monster Category\" is not \"Custom\".\n keep in mind that if the Category does not exist in the DirectorCardCategorySelection, the category will be added and this name will be used. if the Category exists, the card will just be added to it.")]
            public string customCategoryName;

            [Tooltip("The actual DirectorCard for this pair.")]
            public AddressableDirectorCard card;

            /// <summary>
            /// Cast for casting a <see cref="StageMonsterCardPair"/> into a valid <see cref="DirectorAPI.DirectorCardHolder"/>
            /// </summary>
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
