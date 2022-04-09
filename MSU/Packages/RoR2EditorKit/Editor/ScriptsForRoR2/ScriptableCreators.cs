using RoR2;
using RoR2.Skills;
using UnityEditor;
using UnityEngine;
using static RoR2EditorKit.Utilities.ScriptableObjectUtils;
using static RoR2EditorKit.Utilities.AssetDatabaseUtils;

namespace RoR2EditorKit.RoR2Related
{
    /// <summary>
    /// Creation of ScriptableObjects that are normally uncreatable in RoR2
    /// </summary>
    internal static class ScriptableCreators
    {
        #region skilldefs
        [MenuItem("Assets/Create/RoR2/SkillDef/Captain/Orbital")]
        private static void CreateOrbital()
        {
            CreateNewScriptableObject<CaptainOrbitalSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Captain/SupplyDrop")]
        private static void CreateSupplyDrop()
        {
            CreateSkill<CaptainSupplyDropSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Combo")]
        private static void CreateCombo()
        {
            CreateSkill<ComboSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Conditional")]
        private static void CreateConditional()
        {
            CreateSkill<ConditionalSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/EngiMineDeployer")]
        private static void CreateEngiMineDeployer()
        {
            CreateSkill<EngiMineDeployerSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Grounded")]
        private static void CreateGrounded()
        {
            CreateSkill<GroundedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Detonator")]
        private static void CreateDetonator()
        {
            CreateSkill<LunarDetonatorSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Primary")]
        private static void CreatePrimary()
        {
            CreateSkill<LunarPrimaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Secondary")]
        private static void CreateSecondary()
        {
            CreateSkill<LunarSecondaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Stepped")]
        private static void CreateStepped()
        {
            CreateSkill<SteppedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/ToolbotWeapon")]
        private static void CreateToolbotWeapon()
        {
            CreateSkill<ToolbotWeaponSkillDef>();
        }

        private static void CreateSkill<T>() where T : SkillDef
        {
            var skillDef = ScriptableObject.CreateInstance<T>();
            var SO = skillDef as ScriptableObject;
            SO.name = $"New {typeof(T).Name}";
            skillDef = SO as T;

            CreateAssetAtSelectionPath(skillDef);
        }
        #endregion
    }
}