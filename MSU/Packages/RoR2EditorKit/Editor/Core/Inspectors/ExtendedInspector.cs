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
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }

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

        public bool InspectorEnabled { get => InspectorSetting.isEnabled; set => InspectorSetting.isEnabled = value; }

        public override void OnInspectorGUI()
        {
            if (!InspectorEnabled)
            {
                DrawDefaultInspector();
            }
        }

        protected bool CreateEnableInsepctorToggle(string inspectorName) => EditorGUILayout.ToggleLeft($"Enable {ObjectNames.NicifyVariableName(target.GetType().Name)} Inspector", InspectorEnabled);
        protected void DrawField(string propName) => EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), true);
        protected void DrawField(SerializedProperty property, string propName) => EditorGUILayout.PropertyField(property.FindPropertyRelative(propName), true);
        protected void DrawField(SerializedProperty property) => EditorGUILayout.PropertyField(property, true);
        protected void Header(string label) => EditorGUILayout.LabelField(new GUIContent(label), EditorStyles.boldLabel);
        protected void Header(string label, string tooltip) => EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
    }
}
