using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.EditorWindows;
using UnityEditor;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(ItemDisplayDictionary))]
    public class ItemDisplayDictionaryInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement element = new VisualElement();
            Button button = new Button(Open);
            button.text = $"Open ItemDisplayDictionary Editor Window";
            element.Add(button);
            element.Add(new IMGUIContainer(OnInspectorGUI));
            return element;
        }

        private void Open()
        {
            ExtendedEditorWindow.OpenEditorWindow<ItemDisplayDictionaryEditorWindow>(serializedObject);
        }

        [MenuItem("Tools/MSEU/Windows/ItemDisplayDictionary Editor Window")]
        private static void OpenStatic()
        {
            ExtendedEditorWindow.OpenEditorWindow<ItemDisplayDictionaryEditorWindow>(false);
        }
    }
}