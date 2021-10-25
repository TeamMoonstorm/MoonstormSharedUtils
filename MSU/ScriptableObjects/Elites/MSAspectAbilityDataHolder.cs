using RoR2;
using System;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New AspectAbilityDataHolder", menuName = "Moonstorm/Elites/AspectAbilityDataHolder")]
    [Obsolete]
    public class MSAspectAbilityDataHolder : ScriptableObject
    {
        public EquipmentDef equipmentDef;
        public float aiMaxUseDistance;
        public AnimationCurve aiHealthFractionToUseChance;
    }
}
