using RoR2;
using UnityEngine;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New AspectAbilityDataHolder", menuName = "Moonstorm/Elites/AspectAbilityDataHolder")]
    public class MSAspectAbilityDataHolder : ScriptableObject
    {
        public EquipmentDef equipmentDef;
        public float aiMaxUseDistance;
        public AnimationCurve aiHealthFractionToUseChance;
    }
}
