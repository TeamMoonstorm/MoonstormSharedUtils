using MSU.Editor.EditorWindows;
using RoR2.Editor;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor.Inspectors
{
    [UnityEditor.CustomEditor(typeof(ItemDisplayDictionary))]
    public class ItemDisplayDictionaryInspector : IMGUIScriptableObjectInspector<ItemDisplayDictionary>
    {
        protected override void DrawIMGUI()
        {
            EditorGUILayout.HelpBox("ItemDisplayDictionary is Obsolete, Click the button below to open the Upgrade Window.", MessageType.Info);
            if(GUILayout.Button("Upgrade to ItemDisplayAddressedDictionary"))
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