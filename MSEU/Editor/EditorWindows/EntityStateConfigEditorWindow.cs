using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    public class EntityStateConfigEditorWindow : ExtendedEditorWindow
    {
        Vector2 scrollPos = new Vector2();

        static SerializedObject dummySkillDef;
        public static void Open(EntityStateConfiguration esc)
        {
            EntityStateConfigEditorWindow window = GetWindow<EntityStateConfigEditorWindow>("Entity State Configuration Editor");
            window.mainSerializedObject = new SerializedObject(esc);

            dummySkillDef = new SerializedObject(SkillDef.CreateInstance<SkillDef>());
        }

        private void OnGUI()
        {
            var collectionProperty = mainSerializedObject.FindProperty("serializedFieldsCollection");
            var systemTypeProp = mainSerializedObject.FindProperty("targetType");

            mainCurrentProperty = collectionProperty.FindPropertyRelative("serializedFields");

            EditorGUILayout.PropertyField(systemTypeProp.FindPropertyRelative("assemblyQualifiedName"));

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            var tuple = DrawScrollableButtonSidebar(mainCurrentProperty, scrollPos);
            scrollPos = tuple.Item1;

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            if (mainSelectedProperty != null)
            {
                DrawSelectedSerializableFieldPropPanel();
            }
            else
            {
                EditorGUILayout.LabelField("Select a Serializable Field from the List.");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawSelectedSerializableFieldPropPanel()
        {
            mainCurrentProperty = mainSelectedProperty;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(500));

            DrawField("fieldName", true);

            GUILayout.Space(30);

            var fieldValueProp = mainSelectedProperty.FindPropertyRelative("fieldValue");

            var stringValue = fieldValueProp.FindPropertyRelative("stringValue");
            var objValue = fieldValueProp.FindPropertyRelative("objectValue");

            if (!string.IsNullOrEmpty(stringValue.stringValue))
            {
                DrawField(stringValue, true);
            }
            else if (objValue.objectReferenceValue != null)
            {
                DrawField(objValue, true);
            }
            else if (objValue.objectReferenceValue == null && string.IsNullOrEmpty(stringValue.stringValue))
            {
                DrawField(stringValue, true);
                DrawField(objValue, true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}