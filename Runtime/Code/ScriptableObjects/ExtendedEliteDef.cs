using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public class ExtendedEliteDef : EliteDef
    {
        public VanillaTier eliteTier;
        public Texture2D eliteRamp;
        public Material overlayMaterial;
        public GameObject effect;

        [Flags]
        public enum VanillaTier
        {
            None = 0,
            HonorDisabled = 1,
            HonorActive = 2,
            PostLoop = 4,
            Lunar = 8
        }
    }
}
