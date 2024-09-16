using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.ObjectModel;
using R2API.AddressReferencedAssets;
using RoR2.ExpansionManagement;
using static MSU.MonsterCardProvider;
using UnityEngine.Serialization;

namespace MSU
{
    /// <summary>
    /// An InteractableCardProvider is a ScriptableObject which is used to contain all of an Interactable's DirectorCards, which then will be added to the game's stages using <see cref="DirectorAPI"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New InteractableCardProvider", menuName = "MSU/DirectorCardProviders/InteractableCardProvider")]
    public class InteractableCardProvider : ScriptableObject
    {
        /// <summary>
        /// A Dictionary that contains this Interactable's <see cref="DirectorCardHolderExtended"/> for vanilla stages, which can be accessed by giving the corresponding key of type <see cref="DirectorAPI.Stage"/>.
        /// <br>For custom stages, use <see cref="customStageToCards"/></br>
        /// </summary>
        public ReadOnlyDictionary<DirectorAPI.Stage, DirectorCardHolderExtended> stageToCards
        {
            get
            {
                if (_stageToCards == null)
                {
                    BuildDictionaries();
                }
                return _stageToCards;
            }
        }
        private ReadOnlyDictionary<DirectorAPI.Stage, DirectorCardHolderExtended> _stageToCards;

        /// <summary>
        /// A Dictionary that contains this Interactable's <see cref="DirectorCardHolderExtended"/> for custom stages, which can be accessed by giving the corresponding key which would be the stage's name.
        /// <br>For vanilla stages, use <see cref="stageToCards"/></br>
        /// </summary>
        public ReadOnlyDictionary<string, DirectorCardHolderExtended> customStageToCards
        {
            get
            {
                if (_customStageToCards == null)
                {
                    BuildDictionaries();
                }
                return _customStageToCards;
            }
        }
        private ReadOnlyDictionary<string, DirectorCardHolderExtended> _customStageToCards;

        [FormerlySerializedAs("_serializedCardPairs")]
        [Tooltip("Contains your Interactable's Cards.")]
        public StageInteractableCardPair[] serializedCardPairs = Array.Empty<StageInteractableCardPair>();

        /// <summary>
        /// Method that builds a <see cref="HashSet{T}"/> containing all the unique instances of SpawnCards held by this InteractableCardProvider.
        /// <br>Keep in mind that the hash set will not contain SpawnCards obtained from Addressables.</br>
        /// </summary>
        /// <returns>A <see cref="HashSet{T}"/> containing all of the SpawnCards this InteractableCardProvider provides</returns>
        public HashSet<SpawnCard> BuildSpawnCardSet()
        {
            HashSet<SpawnCard> result = new HashSet<SpawnCard>();
            foreach (StageInteractableCardPair pair in serializedCardPairs)
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
        /// Builds the <see cref="stageToCards"/> and the <see cref="customStageToCards"/> dictionaries
        /// </summary>
        public void BuildDictionaries()
        {
            var stageDict = new Dictionary<DirectorAPI.Stage, DirectorCardHolderExtended>();
            var customStageDict = new Dictionary<string, DirectorCardHolderExtended>();

            //Iterate thru pairs
            for(int i = 0; i < serializedCardPairs.Length; i++)
            {
                var pair = serializedCardPairs[i];
                var stageFlags = (DirectorAPI.Stage)pair.stage;

                //Iterate thru stage enum values
                foreach(DirectorAPI.Stage stageEnumValue in Enum.GetValues(typeof(DirectorAPI.Stage)))
                {
                    //If the pair has a custom flag, add said custom stage names to the dictionary.
                    if(stageEnumValue == DirectorAPI.Stage.Custom && stageFlags.HasFlag(stageEnumValue))
                    {
                        if(pair.customStageNames == null)
                        {
                            continue;
                        }

                        //Iterate thru stage names
                        foreach(var stageName in pair.customStageNames)
                        {
                            //If a previous pair already added a card, continue and log warning
                            if(customStageDict.ContainsKey(stageName))
                            {
#if DEBUG
                                MSULog.Warning($"Cannot add {this}'s {i} card pair to custom stage {stageName} because there's already a card pair associated to said stage.");
#endif
                                continue;
                            }
                            customStageDict.Add(stageName, pair);
                        }
                    }

                    //if the pair has any other value thats not custom, add it to the regular dictionary.
                    if (stageEnumValue != DirectorAPI.Stage.Custom && stageFlags.HasFlag(stageEnumValue))
                    {
                        if(stageDict.ContainsKey(stageEnumValue))
                        {
#if DEBUG
                            MSULog.Warning($"Cannot add {this}'s {i} card pair to stage {Enum.GetName(typeof(DirectorAPI.Stage), stageEnumValue)} because there's already a card pair associated to said stage.");
#endif
                            continue;
                        }
                        stageDict.Add(stageEnumValue, pair);
                    }
                }
            }

            _customStageToCards = new ReadOnlyDictionary<string, DirectorCardHolderExtended>(customStageDict);
            _stageToCards = new ReadOnlyDictionary<DirectorAPI.Stage, DirectorCardHolderExtended>(stageDict);
        }

        /// <summary>
        /// Represents a pair of <see cref="DirectorCardHolderExtended"/> and stage metadata.
        /// <br>Contains an implicit cast to cast from this struct to <see cref="DirectorCardHolderExtended"/></br>
        /// </summary>
        [Serializable]
        public struct StageInteractableCardPair
        {
            [Header("Stage Metadata")]
            [Tooltip("The Serializable Stage enum value for this pair, this only includes vanilla stages.\nif you want to add your interactable to a custom stage, set this to \"Custom\" and fill out the field \"customSTageNames\".")]
            public DirectorAPI.StageSerde stage;

            [Tooltip("The custom stage names for this pair, this is only used for custom, non vanilla stages.\nIf you want to add your interactable to a vanilla stage, leave this empty and utilize the field \"Stage Enum\"")]
            public List<string> customStageNames;

            [Header("Director Card Metadata")]
            [Tooltip("The category for this interactable. This is only used for vanilla categories.\nIf you want to add your interactable to a custom category, set this to \"Custom\" and fill out \"Custom Category Weight\" and \"Custom Category Name\"")]
            public DirectorAPI.InteractableCategory interactableCategory;

            [Tooltip("The weight for a new Category, only relevant if \"Interactable Category\" is not \"Custom\".\n keep in mind that if the Category does not exist in the DirectorCardCategorySelection, the category will be added and this weight will be used. if the Category exists, the card will just be added to it.")]
            public float customCategoryWeight;

            [Tooltip("The weight for a new Category, only relevant if \"Interactable Category\" is not \"Custom\".\n keep in mind that if the Category does not exist in the DirectorCardCategorySelection, the category will be added and this name will be used. if the Category exists, the card will just be added to it.")]
            public string customCategoryName;

            [Tooltip("The required expansions for this pair to be picked. ALL the expansions referenced here must be enabled in the run.")]
            public AddressReferencedExpansionDef[] requiredExpansions;


            [Tooltip("The actual DirectorCard for this pair.")]
            public AddressableDirectorCard card;

            /// <summary>
            /// Cast for casting a <see cref="StageInteractableCardPair"/> into a valid <see cref="DirectorCardHolderExtended"/>
            /// </summary>

            public static implicit operator DirectorCardHolderExtended(StageInteractableCardPair other)
            {
                return new DirectorCardHolderExtended
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
                    MonsterCategory = DirectorAPI.MonsterCategory.Invalid,
                    requiredExpansionDefs = other.requiredExpansions.Where(ared => ared.AssetExists).Select(ared => (ExpansionDef)ared).ToArray()
                };
            }
        }
    }
}
