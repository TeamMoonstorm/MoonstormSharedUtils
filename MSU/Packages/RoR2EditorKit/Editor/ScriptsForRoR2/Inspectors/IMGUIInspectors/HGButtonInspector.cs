using RoR2.UI;
using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(HGButton))]
    public class HGButtonInspector : ButtonEditor
    {
        /// <summary>
        /// Access to the Settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        /// <summary>
        /// The setting for this inspector
        /// </summary>
        public EditorInspectorSettings.InspectorSetting InspectorSetting
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
                    var index = Settings.InspectorSettings.inspectorSettings.IndexOf(_inspectorSetting);
                    Settings.InspectorSettings.inspectorSettings[index] = value;
                }
                _inspectorSetting = value;
            }
        }

        private EditorInspectorSettings.InspectorSetting _inspectorSetting;

        /// <summary>
        /// Check if the inspector is enabled
        /// <para>If you're setting the value, and the value is different from the initial value, the inspector will redraw completely to accomodate the new look using either the base inspector or custom inspector</para>
        /// </summary>
        public bool InspectorEnabled
        {
            get
            {
                return InspectorSetting.isEnabled;
            }
            set
            {
                if (value != InspectorSetting.isEnabled)
                {
                    InspectorSetting.isEnabled = value;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            InspectorEnabled = EditorGUILayout.ToggleLeft($"Enable {ObjectNames.NicifyVariableName(target.GetType().Name)} Inspector", InspectorEnabled);
            EditorGUILayout.EndVertical();
            if (!InspectorEnabled)
            {
                base.OnInspectorGUI();
            }
            else
            {
                DrawCustomInspector();
            }
        }

        private void DrawCustomInspector()
        {
            EditorGUILayout.BeginVertical("box");
            Header("Button Settings");
            base.OnInspectorGUI();
            DrawField("onFindSelectableLeft");
            DrawField("onFindSelectableRight");
            DrawField("onSelect");
            DrawField("onDeselect");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            Header("Control Options");
            DrawField("allowAllEventSystems");
            DrawField("submitOnPointerUp");
            DrawField("disablePointerClick");
            DrawField("disableGamepadClick");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            Header("Visual Properties");
            DrawField("imageOnInteractable");
            DrawField("showImageOnHover");
            DrawField("imageOnHover");
            DrawField("updateTextOnHover");
            DrawField("hoverLanguageTextMeshController");
            DrawField("hoverToken");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            Header("Misc Options");
            DrawField("requiredTopLayer");
            DrawField("defaultFallbackButton");
            DrawField("uiClickSoundOverride");
            EditorGUILayout.EndVertical();
        }

        private void Header(string label) => EditorGUILayout.LabelField(new GUIContent(label), EditorStyles.boldLabel);
        private void Header(string label, string tooltip) => EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
        private void DrawField(SerializedProperty prop) => EditorGUILayout.PropertyField(prop, true);
        private void DrawField(string prop) => EditorGUILayout.PropertyField(serializedObject.FindProperty(prop), true);
        private void DrawField(SerializedProperty prop, string propName) => EditorGUILayout.PropertyField(prop.FindPropertyRelative(propName), true);
    }
}
