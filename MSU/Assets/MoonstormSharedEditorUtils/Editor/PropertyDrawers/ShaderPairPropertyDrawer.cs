using RoR2EditorKit.Core.PropertyDrawers;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Moonstorm.EditorUtils.Settings;
using UnityEditor.UIElements;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShaderDictionary.ShaderPair))]
    public sealed class ShaderPairPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            PropertyField prop = new PropertyField(property.FindPropertyRelative("original"));
            Length length = new Length(50, LengthUnit.Percent);
            StyleLength sLength = new StyleLength(length);
            prop.style.width = sLength;
            root.Add(prop);

            prop = new PropertyField(property.FindPropertyRelative("stubbed"));
            prop.style.width = sLength;
            root.Add(prop);
            return root;
        }
    }
}