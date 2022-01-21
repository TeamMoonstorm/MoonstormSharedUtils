using RoR2EditorKit.Core.Inspectors;
using UnityEditor;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ChildLocator))]
    public class ChildLocatorCustomEditor : ComponentInspector
    {
        public override void DrawCustomInspector()
        {
            var array = serializedObject.FindProperty("transformPairs");
            array.arraySize = EditorGUILayout.DelayedIntField("Size", array.arraySize);
            EditorGUI.indentLevel++;
            DrawNameTransformPairs(array);
            EditorGUI.indentLevel--;
        }

        private void DrawNameTransformPairs(SerializedProperty property)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                var prop = property.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                DrawField(prop.FindPropertyRelative("name"));
                DrawField(prop.FindPropertyRelative("transform"));
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}