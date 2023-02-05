using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VanillaSkinDefinition.MSBaseSkin))]
    public class MSBaseSkinPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty skinAddress = property.FindPropertyRelative(nameof(VanillaSkinDefinition.MSBaseSkin.skinAddress));

            TextField textField = new TextField();
            textField.name = property.name;
            textField.label = skinAddress.displayName;
            textField.tooltip = property.displayName;
            textField.BindProperty(skinAddress);
            return textField;
        }
    }
}
