using R2API;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static R2API.DirectorAPI;

namespace Moonstorm
{
    /// <summary>
    /// A <see cref="MSInteractableDirectorCard"/> is an extension of <see cref="InteractableSpawnCard"/>
    /// <para>A MSInteractableDirectorCard can be used by the <see cref="SceneDirector"/> so the supplied interactable can spawn ingame</para>
    /// <para>Used in the <see cref="InteractableBase"/></para>
    /// </summary>
    [CreateAssetMenu(fileName = "New InteractableDirectorCard", menuName = "Moonstorm/Director Cards/Interactable Director Card", order = 5)]
    public class MSInteractableDirectorCard : InteractableSpawnCard
    {
        /// <summary>
        /// An struct used to compare if two <see cref="MSInteractableDirectorCard"/> implement the same Interactable prefab.
        /// </summary>
        public struct PrefabComparer : IEqualityComparer<InteractableSpawnCard>
        {
            /// <summary>
            /// Checks if two InteractableSpawnCard implements the same interactable prefab
            /// </summary>
            /// <returns>True if both cards are not null and implement the same prefab, false otherwise</returns>
            public bool Equals(InteractableSpawnCard x, InteractableSpawnCard y)
            {
                if (!x || !y)
                    return false;

                if (!x.prefab || !y.prefab)
                    return false;

                return x.prefab == y.prefab;
            }

            /// <summary>
            /// Returns a hash code from this prefab comparer.
            /// </summary>
            /// <returns>-1 if the card is null or has no prefab, otherwise it calls the prefab's GetHashCode function</returns>
            public int GetHashCode(InteractableSpawnCard obj)
            {
                if (!obj)
                    return -1;
                if (!obj.prefab)
                    return -1;

                return obj.prefab.GetHashCode();
            }
        }
        [Space(10)]
        [Header("Settings for DirectorAPI")]
        public AddressableDirectorCard addressReferencedDirectorCard = new AddressableDirectorCard();

        [Tooltip("The category for this interactable. If interactableCategory is set to Custom, the option to set a Custom Category will appear.")]
        public InteractableCategory interactableCategory;

        [Tooltip("The name of the custom category. DirectorAPI automatically adds the category if its not present in the dccs")]
        public string customCategory;
        [Tooltip("The weight for the custom category.\nA list of the vanilla weights:" +
            "\nChests: 45" +
            "\nBarrels: 10" +
            "\nShrines: 10" +
            "\nDrones: 14" +
            "\nMisc: 7" +
            "\nRare: 0.4" +
            "\nDuplicator: 8" +
            "\nVoid Stuff: 3")]
        public float customCategoryWeight = 1;

        [Tooltip("The stages where this interactable can spawn")]
        [EnumMask(typeof(R2API.DirectorAPI.Stage))]
        public R2API.DirectorAPI.Stage stages;

        [Tooltip("The list of custom stages where this monster can spawn, adding the keyword \"ALL\" means this interactable can spawn on ANY custom stage")]
        public List<string> customStages = new List<string>();

        [Tooltip("The ExpansionDefs that neeed to be enabled for this Interactable to spawn. note that ALL expansions need to be enabled for the Interactable to spawn")]
        public List<AddressReferencedExpansionDef> requiredExpansionDefs = new List<AddressReferencedExpansionDef>();

        /// <summary>
        /// The DirectorCardHolder for this MSMonsterDirectorCard
        /// </summary>
        public DirectorCardHolder DirectorCardHolder
        {
            get
            {
                if (_directorCardHolder != null)
                {
                    return _directorCardHolder;
                }
                else
                {
                    _directorCardHolder = new DirectorCardHolder();
                    _directorCardHolder.Card = new DirectorCard
                    {
                        forbiddenUnlockableDef = addressReferencedDirectorCard.forbiddenUnlockableDef,
                        minimumStageCompletions = addressReferencedDirectorCard.minimumStageCompletions,
                        preventOverhead = addressReferencedDirectorCard.preventOverhead,
                        requiredUnlockableDef = addressReferencedDirectorCard.requiredUnlockableDef,
                        selectionWeight = addressReferencedDirectorCard.selectionWeight,
                        spawnCard = this,
                        spawnDistance = addressReferencedDirectorCard.spawnDistance
                    };
                    _directorCardHolder.InteractableCategory = interactableCategory;
                    _directorCardHolder.CustomInteractableCategory = customCategory;
                    _directorCardHolder.InteractableCategorySelectionWeight = customCategoryWeight;
                    _directorCardHolder.MonsterCategory = MonsterCategory.Invalid;
                    return _directorCardHolder;
                }
            }
            set
            {
                _directorCardHolder = value;
            }
        }

        private DirectorCardHolder _directorCardHolder = null;

        private void Awake()
        {
            addressReferencedDirectorCard.spawnCard = this as InteractableSpawnCard;
            customStages = customStages.Select(stageName => stageName.ToLowerInvariant()).ToList();
#if !UNITY_EDITOR
            Migrate();
#endif
        }

        /// <summary>
        /// Wether this Monster is available for the current run
        /// </summary>
        /// <param name="expansionDefs">The run's enabled expansions</param>
        /// <returns>True if available, false otherwise</returns>
        public virtual bool IsAvailable(ExpansionDef[] expansionDefs)
        {
            bool available = true;
            var reqExpansions = requiredExpansionDefs.Where(exp => exp.AssetExists);
            foreach (ExpansionDef ed in reqExpansions)
            {
                available = expansionDefs.Contains(ed);
            }
            return available;
        }
    }
}
