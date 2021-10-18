using Moonstorm;
using RoR2;
using RoR2.ContentManagement;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    [CustomEditor(typeof(SerializableContentPack))]
    public class SerializableContentPackCustomEditor : Editor
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            SerializableContentPack obj = EditorUtility.InstanceIDToObject(instanceID) as SerializableContentPack;
            if (obj != null)
            {
                SerializableContentPackEditorWindow.Open(obj);
                return true;
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                SerializableContentPackEditorWindow.Open((SerializableContentPack)target);
            }
        }
    }
}