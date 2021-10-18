using Moonstorm;
using UnityEngine;
using UnityEditor;

namespace Moonstorm.EditorUtils.Editors
{
    public class SIDREditorWindow : ExtendedEditorWindow
    {
        public Vector2 scrollPos = new Vector2();
        private string selectedRulesPath;
        private SerializedProperty selectedRulesProperty;

        public static void Open(MSSingleItemDisplayRule sidr)
        {
            SIDREditorWindow window = GetWindow<SIDREditorWindow>("Single Item Display Ruleset Editor");

            window.mainSerializedObject = new SerializedObject(sidr);
        }

        private void OnGUI()
        {
            mainCurrentProperty = mainSerializedObject.FindProperty("SingleItemDisplayRules");

            DrawField(mainSerializedObject.FindProperty("KeyAssetName"), true);
            DrawField(mainSerializedObject.FindProperty("displayPrefabName"), true);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            var tuple = DrawScrollableButtonSidebar(mainCurrentProperty, scrollPos);
            scrollPos = tuple.Item1;

            if (tuple.Item2)
            {
                selectedRulesPath = null;
                selectedRulesProperty = null;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            if (mainSelectedProperty != null)
            {
                DrawSelectedSingleKeyAssetPropPanel();
            }
            else
            {
                EditorGUILayout.LabelField("Select an item from the List.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawSelectedSingleKeyAssetPropPanel()
        {
            mainCurrentProperty = mainSelectedProperty;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150));

            DrawField("VanillaIDRSKey", true);

            var rules = mainCurrentProperty.FindPropertyRelative("ItemDisplayRules");

            DrawButtonSidebar(rules, ref selectedRulesPath, ref selectedRulesProperty);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));

            if (selectedRulesProperty != null)
            {
                DrawProperties(selectedRulesProperty, true);
            }
            else
            {
                EditorGUILayout.LabelField("Select a Rule from the List.");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}