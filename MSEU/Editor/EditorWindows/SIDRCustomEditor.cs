using Moonstorm;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    [CustomEditor(typeof(MSSingleItemDisplayRule))]
    public class SIDRCustomEditor : Editor
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            MSSingleItemDisplayRule obj = EditorUtility.InstanceIDToObject(instanceID) as MSSingleItemDisplayRule;
            if (obj != null)
            {
                SIDREditorWindow.Open(obj);
                return true;
            }
            return false;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                SIDREditorWindow.Open((MSSingleItemDisplayRule)target);
            }
        }
    }
}