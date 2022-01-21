using RoR2.UI;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(HGButton))]
    public class HGButtonCustomEditor : ButtonEditor
    {
        public EnabledAndDisabledInspectorsSettings.InspectorSetting InspectorSetting
        {
            get
            {
                if (_inspectorSetting == null)
                {
                    var setting = ExtendedInspector.Settings.InspectorSettings.GetOrCreateInspectorSetting(GetType());
                    _inspectorSetting = setting;
                }
                return _inspectorSetting;
            }
            set
            {
                if (_inspectorSetting != value)
                {
                    var index = ExtendedInspector.Settings.InspectorSettings.EnabledInspectors.IndexOf(_inspectorSetting);
                    ExtendedInspector.Settings.InspectorSettings.EnabledInspectors[index] = value;
                }
                _inspectorSetting = value;
            }
        }

        private EnabledAndDisabledInspectorsSettings.InspectorSetting _inspectorSetting;

        public bool InspectorEnabled { get => InspectorSetting.isEnabled; set => InspectorSetting.isEnabled = value; }

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