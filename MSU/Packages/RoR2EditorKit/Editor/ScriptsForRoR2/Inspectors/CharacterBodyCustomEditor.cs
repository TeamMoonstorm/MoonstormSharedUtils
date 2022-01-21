using RoR2EditorKit.Core.Inspectors;
using UnityEditor;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(RoR2.CharacterBody))]
    public class CharacterBodyCustomEditor : ComponentInspector
    {
        bool showingStats = true;
        bool showingLevelStats = true;
        public override void DrawCustomInspector()
        {
            DrawField("baseNameToken");
            DrawField("subtitleNameToken");
            DrawField("bodyFlags");
            var rootMotion = serializedObject.FindProperty("rootMotionInMainState");
            DrawField(rootMotion);
            if (rootMotion.boolValue)
            {
                DrawField("mainRootSpeed");
            }

            showingStats = EditorGUILayout.Foldout(showingStats, "Show Stats");
            if (showingStats)
            {
                EditorGUI.indentLevel++;
                DrawBStats();
                EditorGUI.indentLevel--;
            }

            DrawField("sprintingSpeedMultiplier");
            DrawField("autoCalculateLevelStats");

            showingLevelStats = EditorGUILayout.Foldout(showingLevelStats, "Show Stats");
            if (showingLevelStats)
            {
                EditorGUI.indentLevel++;
                DrawLStats();
                EditorGUI.indentLevel--;
            }

            DrawField("wasLucky");
            DrawField("spreadBloomDecayTime");
            DrawField("spreadBloomCurve");
            DrawField("crosshairPrefab");
            DrawField("aimOriginTransform");
            DrawField("hullClassification");
            DrawField("portraitIcon");
            DrawField("bodyColor");
            DrawField("isChampion");
            DrawField("currentVehicle");
            DrawField("preferredPodPrefab");
            DrawField("preferredInitialStateType");
            DrawField("skinIndex");
            DrawField("customKillTotalStatName");
            DrawField("overrideCoreTransform");
        }

        private void DrawBStats()
        {
            DrawBStat("MaxHealth");
            DrawBStat("Regen");
            DrawBStat("MaxShield");
            DrawBStat("MoveSpeed");
            DrawBStat("Acceleration");
            DrawBStat("JumpPower");
            DrawBStat("Damage");
            DrawBStat("AttackSpeed");
            DrawBStat("Crit");
            DrawBStat("Armor");
            DrawBStat("JumpCount");
        }

        private void DrawLStats()
        {
            DrawLStat("MaxHealth");
            DrawLStat("Regen");
            DrawLStat("MaxShield");
            DrawLStat("MoveSpeed");
            DrawLStat("JumpPower");
            DrawLStat("Damage");
            DrawLStat("AttackSpeed");
            DrawLStat("Crit");
            DrawLStat("Armor");
        }

        private void DrawBStat(string stat)
        {
            DrawField($"base{stat}");
        }

        private void DrawLStat(string levelStat)
        {
            DrawField($"level{levelStat}");
        }
    }
}