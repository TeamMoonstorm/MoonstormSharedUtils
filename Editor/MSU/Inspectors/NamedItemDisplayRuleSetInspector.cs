using MSU.Editor.EditorWindows;
using RoR2.Editor;
using UnityEngine;
using UnityEditor;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(NamedItemDisplayRuleSet))]
    public class NamedItemDisplayRuleSetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("NamedItemDisplayRuleSet is Obsolete, Click the button below to open the Upgrade Window.", MessageType.Info);
            if (GUILayout.Button("Migrate contents to Target ItemDisplayRuleSet"))
            {
                var instance = ItemDisplayMigrationWizard.Open();
                NamedItemDisplayRuleSet target = (NamedItemDisplayRuleSet)serializedObject.targetObject;
                if (!instance.itemsToUpgrade.Contains(target))
                {
                    instance.itemsToUpgrade.Add(target);
                }
                instance.upgradeNamedItemDisplayRuleSet = true;
            }
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }
}