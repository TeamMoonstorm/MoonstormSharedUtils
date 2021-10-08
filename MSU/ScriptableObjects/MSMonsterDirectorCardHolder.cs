using System;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New MonsterDirectorCardHolder", menuName = "Moonstorm/Director Cards/DirectorCardHolder", order = 5)]
    public class MSMonsterDirectorCardHolder : ScriptableObject
    {
        public SceneCardPair[] sceneCardPairs;

        [Serializable]
        public struct SceneCardPair
        {
            public VanillaStages[] vanillaStages;

            /// <summary>Type "all" to include this card in all scenes, or "default" to include it in all scenes not mentioned. Can also be used for modded stages</summary>
            public string[] sceneNameOverrides;
            /// <summary>The name of the category in the scene's director card.</summary>
            public string categoryName;
            public MSMonsterDirectorCard directorCard;
        }

        public enum VanillaStages
        {
            None,
            GolemPlains,
            BlackBeach,
            GooLake,
            FoggySwamp,
            WispGraveyard,
            FrozenWall,
            RootJungle,
            ShipGraveyard,
            DampCave,
            SkyMeadow,
            Moon,
            GoldShores,
            Arena,
            All
        }
    }
}