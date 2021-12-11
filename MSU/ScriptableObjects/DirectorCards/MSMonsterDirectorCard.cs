using RoR2;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New DirectorCardHolder", menuName = "Moonstorm/Director Cards/MonsterDirectorCard", order = 5)]
    public class MSMonsterDirectorCard : ScriptableObject
    {
        public DirectorCard directorCard;
    }
}