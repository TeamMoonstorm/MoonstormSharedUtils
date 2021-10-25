using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    [CustomEditor(typeof(VanillaSkinDef))]
    public class VanillaSkinDefCustomEditor : Editor
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            VanillaSkinDef obj = EditorUtility.InstanceIDToObject(instanceID) as VanillaSkinDef;
            if (obj != null)
            {
                VanillaSkinDefEditorWindow.Open(obj);
                return true;
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                VanillaSkinDefEditorWindow.Open((VanillaSkinDef)target);
            }
        }
    }
}