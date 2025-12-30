using HG.Coroutines;
using RoR2;
using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(VanillaSkinDef))]
    public class VanillaSkinDefInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox($"Due to memory management changes, it is not possible to migrate from VanillaSkinDef to UberSkinDef directly, we apologize for this inconvenience.", MessageType.Info);
            EditorGUI.BeginDisabledGroup(true);
            DrawDefaultInspector();
            EditorGUI.EndDisabledGroup();
        }
    }
}