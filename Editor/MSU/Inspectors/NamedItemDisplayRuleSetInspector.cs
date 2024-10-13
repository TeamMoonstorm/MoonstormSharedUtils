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
            DrawDefaultInspector();
            if(GUILayout.Button("Open NamedItemDisplayRuleSet Editor Window"))
            {
                ExtendedEditorWindow.Open<NamedItemDisplayRuleSetEditorWindow>(targetType);
            }
        }
    }
}