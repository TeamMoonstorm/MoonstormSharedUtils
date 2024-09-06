using MSU.Editor.EditorWindows;
using MSU;
using RoR2.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(NamedItemDisplayRuleSet))]
    public class NamedIDRSInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement element = new VisualElement();
            Button button = new Button(Open);
            button.text = $"Open NamedIDRS Editor Window";
            element.Add(button);
            element.Add(new IMGUIContainer(OnInspectorGUI));
            return element;
        }

        private void Open()
        {
            ExtendedEditorWindow window = ExtendedEditorWindow.Open<NamedIDRSEditorWindow>(serializedObject.targetObject);
        }
        [MenuItem("Tools/MSEU/Windows/NamedIDRS Editor Window")]
        private static void OpenStatic()
        {
            ExtendedEditorWindow.Open<NamedIDRSEditorWindow>();
        }
    }
}
