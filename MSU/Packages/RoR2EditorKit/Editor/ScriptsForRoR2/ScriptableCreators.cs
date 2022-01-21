using RoR2;
using RoR2.Skills;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related
{
    /// <summary>
    /// Creation of ScriptableObjects that are normally uncreatable in RoR2
    /// </summary>
    public static class ScriptableCreators
    {
        [MenuItem("Assets/Create/RoR2/UnlockableDef")]
        public static void CreateUnlockableDef()
        {
            var unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = "New UnlockableDef";
            Util.CreateAssetAtSelectionPath(unlockableDef);
        }

        #region skilldefs
        [MenuItem("Assets/Create/RoR2/SkillDef/Captain/Orbital")]
        public static void CreateOrbital()
        {
            CreateSkill<CaptainOrbitalSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Captain/SupplyDrop")]
        public static void CreateSupplyDrop()
        {
            CreateSkill<CaptainSupplyDropSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Combo")]
        public static void CreateCombo()
        {
            CreateSkill<ComboSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Conditional")]
        public static void CreateConditional()
        {
            CreateSkill<ConditionalSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/EngiMineDeployer")]
        public static void CreateEngiMineDeployer()
        {
            CreateSkill<EngiMineDeployerSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Grounded")]
        public static void CreateGrounded()
        {
            CreateSkill<GroundedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Detonator")]
        public static void CreateDetonator()
        {
            CreateSkill<LunarDetonatorSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Primary")]
        public static void CreatePrimary()
        {
            CreateSkill<LunarPrimaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Secondary")]
        public static void CreateSecondary()
        {
            CreateSkill<LunarSecondaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Stepped")]
        public static void CreateStepped()
        {
            CreateSkill<SteppedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/ToolbotWeapon")]
        public static void CreateToolbotWeapon()
        {
            CreateSkill<ToolbotWeaponSkillDef>();
        }

        private static void CreateSkill<T>() where T : SkillDef
        {
            var skillDef = ScriptableObject.CreateInstance<T>();
            var SO = skillDef as ScriptableObject;
            SO.name = $"New {typeof(T).Name}";
            skillDef = SO as T;

            Util.CreateAssetAtSelectionPath(skillDef);
        }
        #endregion
    }
}