using System.Collections;
using System.Collections.Generic;
using Moonstorm;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    [CustomEditor(typeof(MSIDRS))]
    public class MSIDRSCustomEditor : Editor
    {
        [OnOpenAsset]
        public static bool OpenEditor(int instanceID, int line)
        {
            MSIDRS obj = EditorUtility.InstanceIDToObject(instanceID) as MSIDRS;
            if (obj != null)
            {
                MSIDRSEditorWindow.Open(obj);
                return true;
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                MSIDRSEditorWindow.Open((MSIDRS)target);
            }
        }
    }
}