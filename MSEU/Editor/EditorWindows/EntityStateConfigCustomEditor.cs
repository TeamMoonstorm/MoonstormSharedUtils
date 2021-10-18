using Moonstorm;
using RoR2;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public class EntityStateConfigCustomEditor : Editor
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            EntityStateConfiguration obj = EditorUtility.InstanceIDToObject(instanceID) as EntityStateConfiguration;
            if (obj != null)
            {
                EntityStateConfigEditorWindow.Open(obj);
                return true;
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                EntityStateConfigEditorWindow.Open((EntityStateConfiguration)target);
            }
        }
    }
}
