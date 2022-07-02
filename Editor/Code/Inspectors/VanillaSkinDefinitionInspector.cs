using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(VanillaSkinDefinition))]
    public class VanillaSkinDefinitionInspector : ExtendedInspector<VanillaSkinDefinition>
    {
        protected override bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Packages/teammoonstorm-moonstormsharedutils/Editor");
        }
        protected override void DrawInspectorGUI()
        {
            DrawInspectorElement.Q<PropertyField>("m_Script").SetEnabled(false);
        }
    }
}
