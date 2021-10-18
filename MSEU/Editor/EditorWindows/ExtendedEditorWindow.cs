using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.Editors
{
    public class ExtendedEditorWindow : EditorWindow
    {
        protected SerializedObject mainSerializedObject;
        protected SerializedProperty mainCurrentProperty;

        private string mainSelectedPropertyPath;
        protected SerializedProperty mainSelectedProperty;

        protected void DrawProperties(SerializedProperty property, bool drawChildren)
        {
            string lastPropPath = string.Empty;
            foreach (SerializedProperty prop in property)
            {
                if (prop.isArray && prop.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (prop.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawProperties(prop, drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(lastPropPath) && prop.propertyPath.Contains(lastPropPath))
                    {
                        continue;
                    }
                    lastPropPath = prop.propertyPath;
                    EditorGUILayout.PropertyField(prop, drawChildren);
                }
            }
        }

        #region Button Sidebars
        protected bool DrawButtonSidebar(SerializedProperty property)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button(prop.displayName))
                {
                    mainSelectedPropertyPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
            {
                mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition)
        {
            bool pressed = false;
            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            if (property.arraySize != 0)
            {
                foreach (SerializedProperty prop in property)
                {
                    if (GUILayout.Button(prop.displayName))
                    {
                        mainSelectedPropertyPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
                {
                    mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
                }

            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        protected bool DrawButtonSidebar(SerializedProperty property, string buttonName)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                var p = prop.FindPropertyRelative(buttonName);
                if (p != null && p.objectReferenceValue)
                {
                    if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                    {
                        mainSelectedPropertyPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                else if (GUILayout.Button(prop.displayName))
                {
                    mainSelectedPropertyPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
            {
                mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, string buttonName)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            if (property.arraySize != 0)
            {
                foreach (SerializedProperty prop in property)
                {
                    var p = prop.FindPropertyRelative(buttonName);
                    if (p != null && p.objectReferenceValue)
                    {
                        if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                        {
                            mainSelectedPropertyPath = prop.propertyPath;
                            GUI.FocusControl(null);
                            pressed = true;
                        }
                    }
                    else if (GUILayout.Button(prop.displayName))
                    {
                        mainSelectedPropertyPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
                {
                    mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
                }

            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        protected bool DrawButtonSidebar(SerializedProperty property, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (selectedPropPath != string.Empty)
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(selectedPropPath))
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        protected bool DrawButtonSidebar(SerializedProperty property, string propertyNameForButton, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                var p = prop.FindPropertyRelative(propertyNameForButton);
                if (p != null && p.objectReferenceValue)
                {
                    if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                    {
                        selectedPropPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                else if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (selectedPropPath != string.Empty)
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, string propertyNameForButton, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            foreach (SerializedProperty prop in property)
            {
                var p = prop.FindPropertyRelative(propertyNameForButton);
                if (p != null && p.objectReferenceValue)
                {
                    if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                    {
                        selectedPropPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                else if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(selectedPropPath))
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }
        #endregion

        #region Value Sidebars
        protected void DrawValueSidebar(SerializedProperty property)
        {
            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                DrawField(prop, true);
            }
        }
        #endregion

        protected void DrawField(string propName, bool relative)
        {
            if (relative && mainCurrentProperty != null)
            {
                EditorGUILayout.PropertyField(mainCurrentProperty.FindPropertyRelative(propName), true);
            }
            else if (mainSerializedObject != null)
            {
                EditorGUILayout.PropertyField(mainSerializedObject.FindProperty(propName), true);
            }
        }

        protected void DrawField(SerializedProperty property, bool includeChildren)
        {
            EditorGUILayout.PropertyField(property, includeChildren);
        }

        protected void DrawField(string propName, bool relative, SerializedProperty currentProp, SerializedObject serializedObj)
        {
            if (relative && currentProp != null)
            {
                EditorGUILayout.PropertyField(currentProp.FindPropertyRelative(propName), true);
            }
            else if (mainSerializedObject != null)
            {
                EditorGUILayout.PropertyField(serializedObj.FindProperty(propName), true);
            }
        }

        protected void ApplyChanges()
        {
            mainSerializedObject.ApplyModifiedProperties();
        }
    }
}