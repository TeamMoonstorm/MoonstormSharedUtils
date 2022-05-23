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
using RoR2EditorKit.Utilities;
using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.Core.EditorWindows;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(NamedIDRS))]
    public class NamedIDRSInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement element = new VisualElement();
            //element.Add(new IMGUIContainer(OnInspectorGUI));
            Button button = new Button(OpenWindow);
            button.text = $"Open NamedIDRS Editor Window";
            element.Add(button);
            return element;
        }
        private void OpenWindow()
        {
            ObjectEditingEditorWindow<NamedIDRS>.OpenEditorWindow<NamedIDRSEditorWindow>(target);
        }
    }
}
