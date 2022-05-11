using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    public abstract class MSScriptableObjectInspector<T> : ScriptableObjectInspector<T> where T : ScriptableObject
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                if (container != null)
                {
                    var label = container.Q<Label>("scriptType");
                    if (label != null)
                    {
                        label.text = serializedObject.FindProperty("m_Script").objectReferenceValue.name;
                    }
                }
            };
        }
        protected override bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Assets/MoonstormSharedEditorUtils") || path.StartsWith("Packages/teammoonstorm-moonstormsharededitorutils");
        }
    }
}
