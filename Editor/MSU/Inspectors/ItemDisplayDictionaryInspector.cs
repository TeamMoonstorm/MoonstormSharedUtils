using MSU.Editor.EditorWindows;
using RoR2.Editor;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor.Inspectors
{
    [UnityEditor.CustomEditor(typeof(ItemDisplayDictionary))]
    public class ItemDisplayDictionaryInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("ItemDisplayDictionary is Obsolete, Click the button below to open the Upgrade Window.", MessageType.Info);
            if (GUILayout.Button("Upgrade to ItemDisplayAddressedDictionary"))
            {
                var instance = ItemDisplayMigrationWizard.Open();
                ItemDisplayDictionary target = (ItemDisplayDictionary)serializedObject.targetObject;
                if(!instance.itemsToUpgrade.Contains(target))
                {
                    instance.itemsToUpgrade.Add(target);
                }
                instance.upgradeItemDisplayDictionary = true;
            }
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }
}