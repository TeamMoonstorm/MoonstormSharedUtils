using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.Inspectors
{
    /// <summary>
    /// Inherit from this class to make your own Scriptable Object Inspectors.
    /// </summary>
    /// <typeparam name="T">The type of scriptable object thats being inspected</typeparam>
    public abstract class ScriptableObjectInspector<T> : ExtendedInspector<T> where T : ScriptableObject
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            InspectorEnabled = InspectorSetting.isEnabled;
            finishedDefaultHeaderGUI += DrawEnableToggle;
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                if (container != null)
                {
                    var scriptType = container.Q<Label>("scriptType");
                    if (scriptType != null)
                    {
                        scriptType.text = serializedObject.FindProperty("m_Script").objectReferenceValue.name;
                    }
                }
            };
        }

        private void OnDisable()
        {
            finishedDefaultHeaderGUI -= DrawEnableToggle;
        }

        private void DrawEnableToggle(Editor obj)
        {
            if (obj is ScriptableObjectInspector<T> soInspector)
            {
                InspectorEnabled = EditorGUILayout.ToggleLeft($"Enable {ObjectNames.NicifyVariableName(target.GetType().Name)} Inspector", InspectorEnabled);
            }
        }
    }
}
