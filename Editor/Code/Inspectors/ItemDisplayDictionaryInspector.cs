using MSU.Editor.EditorWindows;
using RoR2.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(ItemDisplayDictionary))]
    public class ItemDisplayDictionaryInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement element = new VisualElement();
            Button button = new Button(Open)
            {
                text = $"Open ItemDisplayDictionary Editor Window"
            };
            element.Add(button);
            element.Add(new IMGUIContainer(OnInspectorGUI));
            return element;
        }

        private void Open()
        {
            ExtendedEditorWindow.Open<ItemDisplayDictionaryEditorWindow>(serializedObject.targetObject);
        }

        [MenuItem("Tools/MoonstormSharedUtils/Windows/ItemDisplayDictionary Editor Window")]
        private static void OpenStatic()
        {
            ExtendedEditorWindow.Open<ItemDisplayDictionaryEditorWindow>(null);
        }
    }
}