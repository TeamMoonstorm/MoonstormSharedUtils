using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Core.Windows;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(VanillaSkinDef))]
    public class VanillaSkinDefInspector : ExtendedInspector
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            if (Settings.InspectorSettings.GetOrCreateInspectorSetting(typeof(VanillaSkinDefInspector)).isEnabled)
            {
                VanillaSkinDef obj = EditorUtility.InstanceIDToObject(instanceID) as VanillaSkinDef;
                if (obj != null)
                {
                    ExtendedEditorWindow.OpenEditorWindow<VanillaSkinDefEditorWindow>(obj, "Vanilla Skin Def Editor Window");
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
                ExtendedEditorWindow.OpenEditorWindow<VanillaSkinDefEditorWindow>(target, "Vanilla Skin Def Editor Window");
            }
        }
    }
}