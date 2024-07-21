using RoR2;
using UnityEngine;
using MSU;
using System.Collections.Generic;
namespace ExampleMod
{
    [CreateAssetMenu(fileName = "MonsterAssetCollection", menuName = "ExampleMod/AssetCollections/MonsterAssetCollection")]
    public class MonsterAssetCollection : BodyAssetCollection
    {
        public MonsterCardProvider monsterCardProvider;
        public R2API.DirectorAPI.DirectorCardHolder dissonanceCardHolder;
    }
}