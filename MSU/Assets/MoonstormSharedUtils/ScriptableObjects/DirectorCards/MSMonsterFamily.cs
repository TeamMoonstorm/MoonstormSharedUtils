using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static R2API.DirectorAPI;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New MonsterFamilyDeck", menuName = "Moonstorm/Director Cards/Monster Family Deck", order = 6)]
    public class MSMonsterFamily : ScriptableObject
    {
        public List<MSMonsterDirectorCard> familyBasicMonsters;
        public List<MSMonsterDirectorCard> familyMiniBosses;
        public List<MSMonsterDirectorCard> familyChampions;

        public float basicMonsterWeight;
        public float miniBossWeight;
        public float championWeight;

        public int minStageCompletion;
        public int maxStageCompletion;
        public float familySelectionWeight;
        public string selectionToken;

        [EnumMask(typeof(R2API.DirectorAPI.Stage))]
        public R2API.DirectorAPI.Stage stages;
        public List<string> customStages = new List<string>();

        public MonsterFamilyHolder MonsterFamilyHolder
        {
            get
            {
                if(_monsterFamilyHolder != null)
                {
                    return _monsterFamilyHolder;
                }
                else
                {
                    _monsterFamilyHolder = new MonsterFamilyHolder
                    {
                        FamilyBasicMonsters = familyBasicMonsters.Select(msmdc => msmdc.directorCard).ToList(),
                        FamilyMinibosses = familyMiniBosses.Select(msmdc => msmdc.directorCard).ToList(),
                        FamilyChampions = familyChampions.Select(msmdc => msmdc.directorCard).ToList(),
                        FamilyBasicMonsterWeight = basicMonsterWeight,
                        FamilyMinibossWeight = miniBossWeight,
                        FamilyChampionWeight = championWeight,
                        MinStageCompletion = minStageCompletion,
                        MaxStageCompletion = maxStageCompletion,
                        FamilySelectionWeight = familySelectionWeight,
                        SelectionChatString = selectionToken
                    };
                    return _monsterFamilyHolder;
                }
            }
            set
            {
                _monsterFamilyHolder = value;
            }
        }

        private MonsterFamilyHolder _monsterFamilyHolder;

        private void Awake()
        {
            customStages = customStages.Select(stageName => stageName.ToLowerInvariant()).ToList();
        }
    }
}
