using MSU.Editor.EditorWindows;
using RoR2.Editor;
using UnityEngine;

namespace MSU.Editor.Inspectors
{
    [UnityEditor.CustomEditor(typeof(ItemDisplayDictionary))]
    public class ItemDisplayDictionaryInspector : IMGUIScriptableObjectInspector<ItemDisplayDictionary>
    {
        protected override void DrawIMGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Open ItemDisplayDictionary Editor Window"))
            {
                ExtendedEditorWindow.Open<ItemDisplayDictionaryEditorWindow>(targetType).SetSourceObject();
            }
        }
    }
}