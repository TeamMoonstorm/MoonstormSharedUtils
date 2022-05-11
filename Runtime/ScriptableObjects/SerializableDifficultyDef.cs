using RoR2;
using System;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete("SerializableDifficultyDef is obsolete, Use R2API's SerializableDifficultyDef")]
    public class SerializableDifficultyDef : ScriptableObject
    {
        public bool countsAsHardMode;
        public float scalingValue;
        public string nameToken;
        public string descriptionToken;
        public string iconPath;
        public Color color;
        public string serverTag;
        public Sprite iconSprite;

        public DifficultyDef CreateDifficultyDef()
        {
            var def = new DifficultyDef(scalingValue, nameToken, iconPath, descriptionToken, color, serverTag, countsAsHardMode);
            def.iconSprite = iconSprite;
            def.foundIconSprite = true; //We set this to true, otherwise the GetIconSprite method in diffucltyDef returns a null sprite, causing a white square.
            return def;
        }
    }
}