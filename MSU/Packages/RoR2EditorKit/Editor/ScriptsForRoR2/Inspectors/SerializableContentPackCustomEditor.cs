using RoR2.ContentManagement;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Core.Windows;
using RoR2EditorKit.RoR2Related.EditorWindows;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(SerializableContentPack))]
    public class SerializableContentPackCustomEditor : ScriptableObjectInspector
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            if (Settings.InspectorSettings.GetOrCreateInspectorSetting(typeof(SerializableContentPackCustomEditor)).isEnabled)
            {
                SerializableContentPack obj = EditorUtility.InstanceIDToObject(instanceID) as SerializableContentPack;
                if (obj != null)
                {
                    ExtendedEditorWindow.OpenEditorWindow<SerializableContentPackEditorWindow>(obj, "Serializable Content Pack Window");
                    return true;
                }
            }
            return false;
        }

        public override void DrawCustomInspector()
        {
            if (GUILayout.Button("Open Editor"))
            {
                ExtendedEditorWindow.OpenEditorWindow<SerializableContentPackEditorWindow>(target, "Serializable Content Pack Window");
            }
        }
    }
}