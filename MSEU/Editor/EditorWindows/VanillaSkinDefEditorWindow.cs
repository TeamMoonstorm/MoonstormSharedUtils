using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    public class VanillaSkinDefEditorWindow : ExtendedEditorWindow
    {
        string selectedArrayPath;

        string selectedArrayElementPath;
        SerializedProperty selectedArrayElementProperty;

        public static void Open(VanillaSkinDef esc)
        {
            VanillaSkinDefEditorWindow window = GetWindow<VanillaSkinDefEditorWindow>("Serializable Content Pack Editor");
            window.mainSerializedObject = new SerializedObject(esc);
        }

        private void OnGUI()
        {
            DrawField(mainSerializedObject.FindProperty("bodyResourcePathKeyword"), true);
            DrawField(mainSerializedObject.FindProperty("icon"), true);
            DrawField(mainSerializedObject.FindProperty("nameToken"), true);
            DrawField(mainSerializedObject.FindProperty("unlockableDef"), true);
            DrawField(mainSerializedObject.FindProperty("rootObject"), true);

            string[] arrays = new string[7] { "baseSkins", "rendererInfos", "gameObjectActivations", "meshReplacements", "customGameObjectActivations", "vanillaProjectileGhostReplacements", "vanillaMinionSkinReplacements" };

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(300), GUILayout.ExpandHeight(true));

            DrawButtonSidebar(arrays);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            if (mainSelectedProperty != null)
            {
                if (mainSelectedProperty.displayName == "Base Skins")
                {
                    DrawBaseSkins();
                }
                else
                {
                    DrawSelectedArray();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Select an Content Element from the List.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawButtonSidebar(string[] fieldNames)
        {
            foreach (string field in fieldNames)
            {
                if (GUILayout.Button(field))
                {
                    selectedArrayPath = mainSerializedObject.FindProperty(field).propertyPath;
                }
            }
            if (!string.IsNullOrEmpty(selectedArrayPath))
            {
                mainSelectedProperty = mainSerializedObject.FindProperty(selectedArrayPath);
            }
        }

        private void DrawBaseSkins()
        {
            mainCurrentProperty = mainSelectedProperty;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(300));

            DrawValueSidebar(mainCurrentProperty);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectedArray()
        {
            mainCurrentProperty = mainSelectedProperty;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150));

            DrawButtonSidebar(mainCurrentProperty, ref selectedArrayElementPath, ref selectedArrayElementProperty);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));

            if (selectedArrayElementProperty != null)
            {
                DrawProperties(selectedArrayElementProperty, true);
            }
            else
            {
                EditorGUILayout.LabelField("Select an Element from the List.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}