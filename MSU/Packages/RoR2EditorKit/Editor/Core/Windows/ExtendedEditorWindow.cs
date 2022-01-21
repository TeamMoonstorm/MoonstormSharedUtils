using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.Windows
{
    /// <summary>
    /// Derive editor windows from this class.
    /// <para>Based on the video "Easy Editor Windows in Unity with Serialized Properties" by "Game Dev Guide"</para>
    /// <para>https://www.youtube.com/watch?v=c_3DXBrH-Is</para>
    /// </summary>
    public abstract class ExtendedEditorWindow : EditorWindow
    {
        /// <summary>
        /// The serialized object being modified.
        /// </summary>
        protected SerializedObject mainSerializedObject;

        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        /// <summary>
        /// Opens the given editor window, and sets the main serialized object.
        /// </summary>
        /// <typeparam name="T">The editor window to open</typeparam>
        /// <param name="unityObject">The object that's being modified</param>
        /// <param name="windowName">The window's name</param>
        public static void OpenEditorWindow<T>(Object unityObject, string windowName) where T : ExtendedEditorWindow
        {
            T window = GetWindow<T>(windowName);
            if (unityObject != null)
                window.mainSerializedObject = new SerializedObject(unityObject);

            window.OnWindowOpened();
        }

        /// <summary>
        /// Finish any initialization here. calling base recommended
        /// </summary>
        protected virtual void OnWindowOpened() { }

        /// <summary>
        /// Draws a serialized property as if it where drawn by the default inspector
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="drawChildren">Wether or not to include children properties</param>
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

        /// <summary>
        /// Creates a list of buttons where each button goes to a child property inside a property array.
        /// </summary>
        /// <param name="property">The property to draw.</param>
        /// <param name="selectedPropPath">A string to store the selected property's path.</param>
        /// <param name="selectedProperty">The SerializedProperty for storing the currently selected property.</param>
        /// <returns>true if any button has been pressed, false otherwise</returns>
        protected bool DrawButtonSidebar(SerializedProperty property, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            if (property.arraySize != 0)
            {
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
            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            return pressed;
        }

        /// <summary>
        /// Creates a list of buttons where each button goes to a child property inside the property parameter, alongside the ability to scroll thru the list.
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="scrollPosition">A Vector2 for storing the scroll position</param>
        /// <param name="selectedPropPath">A string to store the selected property's path.</param>
        /// <param name="selectedProperty">The SerializedProperty for storing the currently selected property.</param>
        /// <returns>A tuple with the current scroll's position, and wether or not any button has been pressed.</returns>
        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, ref string selectedPropPath, ref SerializedProperty selectedProperty)
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
                        selectedPropPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                if (!string.IsNullOrEmpty(selectedPropPath))
                {
                    selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
                }

            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        /// <summary>
        /// Creates a list of buttons where each button goes to a child property inside the property parameter, Alongside the ability to specify the button's name.
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="buttonName">A string to determine the button's name</param>
        /// <param name="selectedPropPath">A string to store the selected property's path.</param>
        /// <param name="selectedProperty">The SerializedProperty for storing the currently selected property.</param>
        /// <returns>true if any button has been pressed, false otherwise.</returns>
        protected bool DrawButtonSidebar(SerializedProperty property, string buttonName, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            if (property.arraySize != 0)
            {
                foreach (SerializedProperty prop in property)
                {
                    var p = prop.FindPropertyRelative(buttonName);
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
            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            return pressed;
        }

        /// <summary>
        /// Creates a list of buttons where each button goes to a child property inside the property parameter, Alongside the ability to specify the button's name & scroll thru the list
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="scrollPosition">A Vector2 for storing the scroll position</param>
        /// <param name="buttonName">A string to determine the button's name</param>
        /// <param name="selectedPropPath">A string to store the selected property's path.</param>
        /// <param name="selectedProperty">The SerializedProperty for storing the currently selected property.</param>
        /// <returns>A tuple with the current scroll's position, and wether or not any button has been pressed.</returns>
        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, string buttonName, ref string selectedPropPath, ref SerializedProperty selectedProperty)
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

            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }
        #endregion

        #region Value Sidebars
        /// <summary>
        /// Draws a sidebar with the values inside a property array.
        /// </summary>
        /// <param name="property">The property to draw.</param>
        protected void DrawValueSidebar(SerializedProperty property)
        {
            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                DrawField(prop, true);
            }
        }
        #endregion

        #region Button Creation

        /// <summary>
        /// Shorthand for creating a button.
        /// </summary>
        /// <param name="buttonName">The name of the button</param>
        /// <param name="options">GUILayout Options</param>
        /// <returns>True if pressed, false otherwise.</returns>
        protected bool SimpleButton(string buttonName, params GUILayoutOption[] options)
        {
            return GUILayout.Button(buttonName, options);
        }

        /// <summary>
        /// Shorthand for creating a button that switches a bool.
        /// </summary>
        /// <param name="buttonName">The name of the button</param>
        /// <param name="switchingBool">The bool to store the switch</param>
        /// <param name="options">GUILayout Options</param>
        /// <returns>True if Pressed, false Otherwise.</returns>
        protected bool SwitchButton(string buttonName, ref bool switchingBool, params GUILayoutOption[] options)
        {
            var button = GUILayout.Button(buttonName, options);
            if (button)
                switchingBool = !switchingBool;

            return button;
        }
        #endregion

        /// <summary>
        /// Draws a property with the option to add a custom label
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="includeChildren">Wether or not to include its children</param>
        /// <param name="label">The label to use, optional.</param>
        protected void DrawField(SerializedProperty property, bool includeChildren, string label = null)
        {
            if (label != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
            else
                EditorGUILayout.PropertyField(property, includeChildren);
        }

        /// <summary>
        /// Draws a field directly from the main serialized object.
        /// </summary>
        /// <param name="propName">The field to draw.</param>
        protected void DrawField(string propName)
        {
            EditorGUILayout.PropertyField(mainSerializedObject.FindProperty(propName));
        }
        /// <summary>
        /// Draws a field directly from the serialized object given.
        /// </summary>
        /// <param name="propName">The name of the property to draw</param>
        /// <param name="serializedObject">The serialized object that contains the property</param>
        protected void DrawField(string propName, SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propName));
        }

        protected void ApplyChanges()
        {
            mainSerializedObject.ApplyModifiedProperties();
        }
    }
}