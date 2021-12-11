using RoR2;
using System;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New AspectAbilityDataHolder", menuName = "Moonstorm/Elites/AspectAbilityDataHolder")]
    [Obsolete("MSU no longer handles automatic usage of the MSAspectAbilityDataHolder, implementation is up to the end user.")]
    public class MSAspectAbilityDataHolder : ScriptableObject
    {
        public EquipmentDef equipmentDef;
        public float aiMaxUseDistance;
        public AnimationCurve aiHealthFractionToUseChance;
    }
}
