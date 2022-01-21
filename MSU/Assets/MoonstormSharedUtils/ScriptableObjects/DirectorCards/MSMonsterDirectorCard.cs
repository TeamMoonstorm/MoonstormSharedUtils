using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static R2API.DirectorAPI;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New MonsterDirectorCard", menuName = "Moonstorm/Director Cards/MonsterDirectorCard", order = 5)]
    public class MSMonsterDirectorCard : CharacterSpawnCard
    {
        [Space(10)]
        [Header("Settings for DirectorAPI")]
        public DirectorCard directorCard;
        public MonsterCategory monsterCategory;
        [EnumMask(typeof(R2API.DirectorAPI.Stage))]
        public R2API.DirectorAPI.Stage stages;
        public List<string> customStages = new List<string>();

        public DirectorCardHolder DirectorCardHolder
        {
            get
            {
                if(_directorCardHolder != null)
                {
                    return _directorCardHolder;
                }
                else
                {
                    _directorCardHolder = new DirectorCardHolder();
                    _directorCardHolder.Card = directorCard;
                    _directorCardHolder.MonsterCategory = monsterCategory;
                    _directorCardHolder.InteractableCategory = InteractableCategory.None;
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
            base.Awake();
            directorCard.spawnCard = this as CharacterSpawnCard;
            customStages = customStages.Select(stageName => stageName.ToLowerInvariant()).ToList();
        }
    }
}