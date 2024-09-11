using RoR2.Editor;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShaderDictionary.ShaderPair))]
    public class ShaderPairPropertyDrawer : IMGUIPropertyDrawer<ShaderDictionary.ShaderPair>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var posForYaml = position;
            posForYaml.width /= 2;
            var yaml = property.FindPropertyRelative(nameof(ShaderDictionary.ShaderPair.yaml));
            DrawPropertyFieldWithSnugLabel(posForYaml, yaml, yaml.GetGUIContent());

            var posForHlsl = position;
            posForHlsl.width /= 2;
            posForHlsl.x = posForYaml.xMax;
            var hlsl = property.FindPropertyRelative(nameof(ShaderDictionary.ShaderPair.hlsl));
            DrawPropertyFieldWithSnugLabel(posForHlsl, hlsl, hlsl.GetGUIContent());
        }
    }
}