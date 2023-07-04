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
    /// A <see cref="MSMonsterDirectorCard"/> is an extension of <see cref="CharacterSpawnCard"/>.
    /// <para>A MSMonsterDirectorCard can be used by the <see cref="CombatDirector"/> so the supplied Monster can spawn ingame</para>
    /// <para>Used in the <see cref="MonsterBase"/></para>
    /// </summary>
    [CreateAssetMenu(fileName = "New MonsterDirectorCard", menuName = "Moonstorm/Director Cards/MonsterDirectorCard", order = 5)]
    public class MSMonsterDirectorCard : CharacterSpawnCard
    {
        [Space(10)]
        [Header("Settings for DirectorAPI")]
        public DirectorCard directorCard = new DirectorCard();

        [Tooltip("The category for this monster. If MonsterCategory is set to Custom, the option to set a Custom Category will appear")]
        public MonsterCategory monsterCategory;

        [Tooltip("The name of the custom category. DirectorAPI automatically adds the category if its not present in the dccs")]
        public string customCategory;
        [Tooltip("The weight for the custom category.\nA list of the vanilla weights:" +
            "\nBasic Monsters: 4" +
            "\nMinibosses: 2" +
            "\nChampions: 2" +
            "\nSpecial: 1")]
        public float customCategoryWeight = 1;

        [Tooltip("The stages where this monster can spawn")]
        [EnumMask(typeof(R2API.DirectorAPI.Stage))]
        public R2API.DirectorAPI.Stage stages;

        [Tooltip("The list  of custom stages where this monster can spawn")]
        public List<string> customStages = new List<string>();

        [Tooltip("The ExpansionDefs that neeed to be enabled for this Monster to spawn. note that ALL expansions need to be enabled. for the monster to spawn")]
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
                    _directorCardHolder.MonsterCategory = monsterCategory;
                    _directorCardHolder.CustomMonsterCategory = customCategory;
                    _directorCardHolder.MonsterCategorySelectionWeight = customCategoryWeight;
                    _directorCardHolder.InteractableCategory = InteractableCategory.Invalid;
                    return _directorCardHolder;
                }
            }
            set
            {
                _directorCardHolder = value;
            }
        }
        private DirectorCardHolder _directorCardHolder = null;

        private new void Awake()
        {
            base.Awake();
            directorCard.spawnCard = this as CharacterSpawnCard;
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