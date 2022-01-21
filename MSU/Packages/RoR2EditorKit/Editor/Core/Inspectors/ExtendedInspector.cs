using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.Inspectors
{
    /// <summary>
    /// Base inspector for all the RoR2EditorKit Inspectors.
    /// <para>If you want to make a Scriptable Object Inspector, you'll probably want to use the ScriptableInspector</para>
    /// <para>If you want to make an Inspector for a Component, you'll probably want to use the ComponentInspector</para>
    /// </summary>
    public abstract class ExtendedInspector : Editor
    {
        /// <summary>
        /// Access to the Settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        /// <summary>
        /// The setting for this inspector
        /// </summary>
        public EnabledAndDisabledInspectorsSettings.InspectorSetting InspectorSetting
        {
            get
            {
                if (_inspectorSetting == null)
                {
                    var setting = Settings.InspectorSettings.GetOrCreateInspectorSetting(GetType());
                    _inspectorSetting = setting;
                }
                return _inspectorSetting;
            }
            set
            {
                if (_inspectorSetting != value)
                {
                    var index = Settings.InspectorSettings.EnabledInspectors.IndexOf(_inspectorSetting);
                    Settings.InspectorSettings.EnabledInspectors[index] = value;
                }
                _inspectorSetting = value;
            }
        }

        private EnabledAndDisabledInspectorsSettings.InspectorSetting _inspectorSetting;

        /// <summary>
        /// Get or Set the setting for this inspector
        /// </summary>
        public bool InspectorEnabled { get => InspectorSetting.isEnabled; set => InspectorSetting.isEnabled = value; }

        public override void OnInspectorGUI()
        {
            if (!InspectorEnabled)
            {
                DrawDefaultInspector();
            }
            else
            {
                DrawCustomInspector();
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw your Custom Inspector here.
        /// </summary>
        public abstract void DrawCustomInspector();


        /// <summary>
        /// Creates a simple toggle left for enabling or disabling this inspector
        /// </summary>
        /// <returns>True if the inspector is enabled, false otherwise</returns>
        protected bool CreateEnableInsepctorToggle() => EditorGUILayout.ToggleLeft($"Enable {ObjectNames.NicifyVariableName(target.GetType().Name)} Inspector", InspectorEnabled);
        /// <summary>
        /// Draws a property field using the given property name
        /// <para>The property will be found from the serialized object that's being inspected</para>
        /// </summary>
        /// <param name="propName">The property's name</param>
        protected void DrawField(string propName) => EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), true);
        /// <summary>
        /// Draws a property field using the given property name
        /// <para>The property will be found from the given SerializedProperty</para>
        /// </summary>
        /// <param name="property">The property to search in</param>
        /// <param name="propName">The property to find and draw</param>
        protected void DrawField(SerializedProperty property, string propName) => EditorGUILayout.PropertyField(property.FindPropertyRelative(propName), true);
        /// <summary>
        /// Draws a property field using the given property
        /// </summary>
        /// <param name="property">The property to draw</param>
        protected void DrawField(SerializedProperty property) => EditorGUILayout.PropertyField(property, true);
        /// <summary>
        /// Creates a Header for the inspector
        /// </summary>
        /// <param name="label">The text for the label used in this header</param>
        protected void Header(string label) => EditorGUILayout.LabelField(new GUIContent(label), EditorStyles.boldLabel);

        /// <summary>
        /// Creates a Header with a tooltip for the inspector
        /// </summary>
        /// <param name="label">The text for the label used in this header</param>
        /// <param name="tooltip">A tooltip that's displayed after the mouse hovers over the label</param>
        protected void Header(string label, string tooltip) => EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
    }
}
