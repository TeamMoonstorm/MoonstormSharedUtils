using Moonstorm.AddressableAssets;
using RoR2;
using RoR2.ExpansionManagement;
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
        [Space(10)]
        [Header("Settings for DirectorAPI")]
        public DirectorCard directorCard;

        [Tooltip("The category for this interactable")]
        public InteractableCategory interactableCategory;

        [Tooltip("The name of the custom category")]
        public string customCategory;

        [Tooltip("The stages where this monster can spawn")]
        [EnumMask(typeof(R2API.DirectorAPI.Stage))]
        public R2API.DirectorAPI.Stage stages;

        [Tooltip("The list  of custom stages where this monster can spawn")]
        public List<string> customStages = new List<string>();

        [Tooltip("The ExpansionDefs that neeed to be enabled for this Interactable to spawn. note that ALL expansions need to be enabled for the Interactable to spawn")]
        public List<AddressableExpansionDef> requiredExpansions;

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
                    _directorCardHolder.Card = directorCard;
                    _directorCardHolder.InteractableCategory = interactableCategory;
                    _directorCardHolder.CustomInteractableCategory = customCategory;
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
            directorCard.spawnCard = this as InteractableSpawnCard;
            customStages = customStages.Select(stageName => stageName.ToLowerInvariant()).ToList();
        }

        /// <summary>
        /// Wether this Monster is available for the current run
        /// </summary>
        /// <param name="expansionDefs">The run's enabled expansions</param>
        /// <returns>True if available, false otherwise</returns>
        public virtual bool IsAvailable(ExpansionDef[] expansionDefs)
        {
            bool available = true;
            var reqExpansions = requiredExpansions.Where(exp => exp.Asset != null).Select(exp => exp.Asset);
            foreach (ExpansionDef ed in reqExpansions)
            {
                available = expansionDefs.Contains(ed);
            }
            return available;
        }
    }
}
