using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Core.Windows;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(MSIDRS))]
    public class MSIDRSInspector : ExtendedInspector
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            if(Settings.InspectorSettings.GetOrCreateInspectorSetting(typeof(MSIDRSInspector)).isEnabled)
            {
                MSIDRS obj = EditorUtility.InstanceIDToObject(instanceID) as MSIDRS;
                if (obj != null)
                {
                    ExtendedEditorWindow.OpenEditorWindow<MSIDRSEditorWindow>(obj, "String Item Display Rule Set Editor Window");
                    return true;
                }
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (InspectorEnabled && GUILayout.Button("Open Editor"))
            {
                ExtendedEditorWindow.OpenEditorWindow<MSIDRSEditorWindow>(target, "String Item Display Rule Set Editor Window");
            }
        }
    }
}