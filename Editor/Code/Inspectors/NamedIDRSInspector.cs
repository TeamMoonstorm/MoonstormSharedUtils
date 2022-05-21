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
            Button button = new Button(OpenWindow);
            button.text = $"Open NamedIDRS Editor Window";
            return button;
        }
        private void OpenWindow()
        {
            ExtendedEditorWindow<NamedIDRS>.OpenEditorWindow<NamedIDRSEditorWindow>(target);
        }
    }
}
