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
    public class NamedIDRSInspector : IMGUIToVisualElementInspector<NamedIDRS>
    {
        protected override void FinishGUI()
        {
            Button button = new Button();
            button.text = $"Open NamedIDRS Editor Window";
            button.clicked += OpenWindow;

            RootVisualElement.Add(button);
        }

        private void OpenWindow()
        {
            ExtendedEditorWindow<NamedIDRS>.OpenEditorWindow<NamedIDRSEditorWindow>(target);
        }
    }
}
