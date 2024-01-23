using MSU.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShaderDictionary.ShaderPair))]
    public sealed class ShaderPairPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            PropertyField prop = new PropertyField(property.FindPropertyRelative("yaml"));
            Length length = new Length(50, LengthUnit.Percent);
            StyleLength sLength = new StyleLength(length);
            prop.style.width = sLength;
            root.Add(prop);

            prop = new PropertyField(property.FindPropertyRelative("hlsl"));
            prop.style.width = sLength;
            root.Add(prop);
            return root;
        }
    }
}