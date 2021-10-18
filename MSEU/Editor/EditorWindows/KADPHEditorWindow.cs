using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Moonstorm;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    public class KADPHEditorWindow : ExtendedEditorWindow
    {
        public Vector2 scrollPos = new Vector2();
        public static void Open(KeyAssetDisplayPairHolder obj)
        {
            KADPHEditorWindow window = GetWindow<KADPHEditorWindow>("Key Asset Display Pair Holder Editor");
            window.mainSerializedObject = new SerializedObject(obj);
        }

        private void OnGUI()
        {
            mainCurrentProperty = mainSerializedObject.FindProperty("KeyAssetDisplayPairs");

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            var tuple = DrawScrollableButtonSidebar(mainCurrentProperty, scrollPos, "keyAsset");
            scrollPos = tuple.Item1;

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            if (mainSelectedProperty != null)
            {
                DrawSelectedKADP();
            }
            else
            {
                EditorGUILayout.LabelField("Select a Key Asset Display Pair from the List.");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawSelectedKADP()
        {
            mainCurrentProperty = mainSelectedProperty;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(500));

            DrawField("keyAsset", true);

            DrawValueSidebar(mainCurrentProperty.FindPropertyRelative("displayPrefabs"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}