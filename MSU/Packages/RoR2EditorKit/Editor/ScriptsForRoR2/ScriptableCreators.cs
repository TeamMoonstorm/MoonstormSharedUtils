using RoR2;
using RoR2.Skills;
using UnityEditor;
using UnityEngine;
using static RoR2EditorKit.Utilities.ScriptableObjectUtils;
using static RoR2EditorKit.Utilities.AssetDatabaseUtils;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.RoR2Related
{
    using static ScriptableObjectUtils;
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
            CreateNewScriptableObject<CaptainSupplyDropSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Combo")]
        private static void CreateCombo()
        {
            CreateNewScriptableObject<ComboSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Conditional")]
        private static void CreateConditional()
        {
            CreateNewScriptableObject<ConditionalSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/EngiMineDeployer")]
        private static void CreateEngiMineDeployer()
        {
            CreateNewScriptableObject<EngiMineDeployerSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Grounded")]
        private static void CreateGrounded()
        {
            CreateNewScriptableObject<GroundedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Detonator")]
        private static void CreateDetonator()
        {
            CreateNewScriptableObject<LunarDetonatorSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Primary")]
        private static void CreatePrimary()
        {
            CreateNewScriptableObject<LunarPrimaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Secondary")]
        private static void CreateSecondary()
        {
            CreateNewScriptableObject<LunarSecondaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Stepped")]
        private static void CreateStepped()
        {
            CreateNewScriptableObject<SteppedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/ToolbotWeapon")]
        private static void CreateToolbotWeapon()
        {
            CreateNewScriptableObject<ToolbotWeaponSkillDef>();
        }
        #endregion
    }
}