using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New EffectDef Holder", menuName = "Moonstorm/EffectDef Holder", order = 2)]
    [Obsolete("EffectDefHolder is no longer used nor supported, if you want to load effectDefs into the game, use AssetLoader.LoadEffectDefsFromPrefabs()")]
    public class EffectDefHolder : ScriptableObject
    {
        public GameObject[] effectPrefabs;

        public static EffectDef ToEffectDef(GameObject obj)
        {
            return new EffectDef(obj);
        }

        public EffectDef[] ToEffectDefs()
        {
            return effectPrefabs.Select(go => new EffectDef(go)).ToArray();
        }

    }
}