using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.Core.EditorWindows;
using UnityEditor;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(NamedIDRS))]
    public class NamedIDRSInspector : Editor
    {
        private static NamedIDRSEditorWindow Instance;
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
            if (Instance == null)
            {
                Instance = ObjectEditingEditorWindow<NamedIDRS>.OpenEditorWindow<NamedIDRSEditorWindow>(target);
            }
            Instance.Focus();
        }
    }
}
