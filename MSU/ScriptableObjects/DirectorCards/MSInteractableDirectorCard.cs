using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static R2API.DirectorAPI;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New InteractableDirectorCard", menuName = "Moonstorm/Director Cards/Interactable Director Card", order = 5)]
    public class MSInteractableDirectorCard : InteractableSpawnCard
    {
        [Space(10)]
        [Header("Settings for DirectorAPI")]
        public DirectorCard directorCard;
        public InteractableCategory interactableCategory;
        [EnumMask(typeof(R2API.DirectorAPI.Stage))]
        public R2API.DirectorAPI.Stage stages;
        public List<string> customStages = new List<string>();

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
                    _directorCardHolder.MonsterCategory = MonsterCategory.None;
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
    }
}
