using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.EditorWindows;
using UnityEditor;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(NamedIDRS))]
    public class NamedIDRSInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement element = new VisualElement();
            //element.Add(new IMGUIContainer(OnInspectorGUI));
            Button button = new Button(Open);
            button.text = $"Open NamedIDRS Editor Window";
            element.Add(button);
            return element;
        }

        private void Open()
        {
            ExtendedEditorWindow window = ExtendedEditorWindow.OpenEditorWindow<NamedIDRSEditorWindow>(serializedObject);
        }
        [MenuItem("Tools/MSEU/Windows/NamedIDRS Editor Window")]
        private static void OpenStatic()
        {
            ExtendedEditorWindow.OpenEditorWindow<NamedIDRSEditorWindow>(false);
        }
    }
}
