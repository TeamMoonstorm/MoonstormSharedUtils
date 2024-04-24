using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.ObjectModel;
using static MSU.MonsterCardProvider;

namespace MSU
{
    /// <summary>
    /// An InteractableCardProvider is a ScriptableObject which is used to contain all of an Interactable's DirectorCards, which then will be added to the game's stages using <see cref="DirectorAPI"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New InteractableCardProvider", menuName = "MSU/DirectorCardProviders/InteractableCardProvider")]
    public class InteractableCardProvider : ScriptableObject
    {
        /// <summary>
        /// A Dictionary that contains this Interactable's <see cref="DirectorAPI.DirectorCardHolder"/> for vanilla stages, which can be accessed by giving the corresponding key of type <see cref="DirectorAPI.Stage"/>.
        /// <br>For custom stages, use <see cref="CustomStageToCards"/></br>
        /// </summary>
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

        /// <summary>
        /// A Dictionary that contains this Interactable's <see cref="DirectorAPI.DirectorCardHolder"/> for custom stages, which can be accessed by giving the corresponding key which would be the stage's name.
        /// <br>For vanilla stages, use <see cref="StageToCards"/></br>
        /// </summary>
        public ReadOnlyDictionary<string, DirectorAPI.DirectorCardHolder> CustomStageToCards
        {
            get
            {
                if (_customStageToCards == null)
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

        [Tooltip("Contains your Interactable's Cards.")]
        [SerializeField]
        private StageInteractableCardPair[] _serializedCardPairs = Array.Empty<StageInteractableCardPair>();

        /// <summary>
        /// Method that builds a <see cref="HashSet{T}"/> containing all the unique instances of SpawnCards held by this InteractableCardProvider.
        /// <br>Keep in mind that the hash set will not contain SpawnCards obtained from Addressables.</br>
        /// </summary>
        /// <returns>A <see cref="HashSet{T}"/> containing all of the SpawnCards this InteractableCardProvider provides</returns>
        public HashSet<SpawnCard> BuildSpawnCardSet()
        {
            HashSet<SpawnCard> result = new HashSet<SpawnCard>();
            foreach (StageInteractableCardPair pair in _serializedCardPairs)
            {
                var card = pair.card;

                if (card == null)
                    continue;

                var spawnCard = card.spawnCard;
                if (!spawnCard.AssetExists)
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
        public struct StageInteractableCardPair
        {
            [Header("Stage Metadata")]
            [Tooltip("The stage enum for this pair, this only includes vanilla stages.\nif you want to add your interactable to a custom stage, set this to \"Custom\" and fill out the field \"Custom Stage Name\"")]
            public DirectorAPI.Stage stageEnum;
            [Tooltip("The custom stage name for this pair, this is only used for custom, non vanilla stages.\nIf you want to add your interactable to a vanilla stage, leave this empty and utilize the field \"Stage Enum\"")]
            public string customStageName;

            [Header("Director Card Metadata")]
            [Tooltip("The category for this interactable. This is only used for vanilla categories.\nIf you want to add your interactable to a custom category, set this to \"Custom\" and fill out \"Custom Category Weight\" and \"Custom Category Name\"")]
            public DirectorAPI.InteractableCategory interactableCategory;

            [Tooltip("The weight for a new Category, only relevant if \"Interactable Category\" is not \"Custom\".\n keep in mind that if the Category does not exist in the DirectorCardCategorySelection, the category will be added and this weight will be used. if the Category exists, the card will just be added to it.")]
            public float customCategoryWeight;

            [Tooltip("The weight for a new Category, only relevant if \"Interactable Category\" is not \"Custom\".\n keep in mind that if the Category does not exist in the DirectorCardCategorySelection, the category will be added and this name will be used. if the Category exists, the card will just be added to it.")]
            public string customCategoryName;

            [Tooltip("The actual DirectorCard for this pair.")]
            public AddressableDirectorCard card;

            /// <summary>
            /// Cast for casting a <see cref="StageInteractableCardPair"/> into a valid <see cref="DirectorAPI.DirectorCardHolder"/>
            /// </summary>

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
