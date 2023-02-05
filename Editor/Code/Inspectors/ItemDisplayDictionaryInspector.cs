using Moonstorm.EditorUtils.EditorWindows;
using RoR2EditorKit.Core.EditorWindows;
using UnityEditor;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(ItemDisplayDictionary))]
    public class ItemDisplayDictionaryInspector : Editor
    {
        private static ItemDisplayDictionaryEditorWindow Instance;
        public override VisualElement CreateInspectorGUI()
        {
            Button button = new Button(() =>
            {
                if (Instance == null)
                {
                    Instance = ObjectEditingEditorWindow<ItemDisplayDictionary>.OpenEditorWindow<ItemDisplayDictionaryEditorWindow>(target);
                }
                Instance.Focus();
            });
            button.text = $"Open ItemDisplayDictionary Editor Window";
            return button;
        }
    }
}