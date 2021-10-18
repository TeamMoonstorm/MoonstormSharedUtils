using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Moonstorm;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    public class MSIDRSEditorWindow : ExtendedEditorWindow
    {
        public Vector2 scrollPos = new Vector2();

        public string selectedRulePath;
        public SerializedProperty selectedRuleProperty;
        public static void Open(MSIDRS idrs)
        {
            MSIDRSEditorWindow window = GetWindow<MSIDRSEditorWindow>("Moonstorm Item Display Ruleset Editor");
            window.mainSerializedObject = new SerializedObject(idrs);
        }

        private void OnGUI()
        {
            mainCurrentProperty = mainSerializedObject.FindProperty("SS2KeyAssetRuleGroups");

            DrawField(mainSerializedObject.FindProperty("VanillaIDRSKey"), true);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            var tuple = DrawScrollableButtonSidebar(mainCurrentProperty, scrollPos);
            scrollPos = tuple.Item1;

            if (tuple.Item2)
            {
                selectedRulePath = null;
                selectedRuleProperty = null;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            if (mainSelectedProperty != null)
            {
                DrawSelectedKeyAssetPropPanel();
            }
            else
            {
                EditorGUILayout.LabelField("Select a Key Asset Rule Group from the List.");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawSelectedKeyAssetPropPanel()
        {
            mainCurrentProperty = mainSelectedProperty;
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150));

            DrawField("keyAssetName", true);

            var rules = mainCurrentProperty.FindPropertyRelative("rules");

            DrawButtonSidebar(rules, ref selectedRulePath, ref selectedRuleProperty);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));

            if (selectedRuleProperty != null)
            {
                DrawProperties(selectedRuleProperty, true);
            }
            else
            {
                EditorGUILayout.LabelField("Select a Rule from the list.");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }

        /*private void DrawSelectedRule()
        {
            currentRule = selectedRule;

            EditorGUILayout.BeginHorizontal("box");
            DrawField("ruleType", true, currentRule, mainSerializedObject);
            DrawField("displayPrefabName", true, currentRule, mainSerializedObject);
            DrawField("IDPHValues", true, currentRule, mainSerializedObject);
            DrawField("limbMask", true, currentRule, mainSerializedObject);
        }*/

        /*protected void DrawRuleSidebar(SerializedProperty property)
        {
            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button("Rule"))
                {
                    selectedRulePath = prop.propertyPath;
                }
            }
            if (!string.IsNullOrEmpty(selectedRulePath))
            {
                selectedRule = mainSerializedObject.FindProperty(selectedRulePath);
            }
        }*/
    }
}