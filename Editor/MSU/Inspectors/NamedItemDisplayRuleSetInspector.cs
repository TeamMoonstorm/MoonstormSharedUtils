using MSU.Editor.EditorWindows;
using RoR2.Editor;
using UnityEngine;
using UnityEditor;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(NamedItemDisplayRuleSet))]
    public class NamedItemDisplayRuleSetInspector : IMGUIScriptableObjectInspector<NamedItemDisplayRuleSet>
    {
        protected override void DrawIMGUI()
        {
            EditorGUILayout.HelpBox("NamedItemDisplayRuleSet is Obsolete, Click the button below to open the Upgrade Window.", MessageType.Info);
            if (GUILayout.Button("Upgrade to ItemDisplayRuleSet"))
            {
                var instance = ItemDisplayMigrationWizard.Open();
                instance.itemsToUpgrade.Add(targetType);
            }

            EditorGUI.BeginDisabledGroup(true);
            DrawDefaultInspector();
            EditorGUI.EndDisabledGroup();
        }
    }
}