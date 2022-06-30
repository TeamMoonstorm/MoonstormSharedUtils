using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Core.EditorWindows;
using Moonstorm.EditorUtils.EditorWindows;

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
                if(Instance == null)
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