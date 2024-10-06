using UnityEditor;
using UnityEngine;

namespace MSU.Editor.ShaderSystem
{
    [InitializeOnLoad]
    public class MaterialEditorHandleStubbedAndYamlShaders
    {
        static MaterialEditorHandleStubbedAndYamlShaders()
        {
            MSU.Editor.Inspectors.MaterialEditorAdditions.onDraw += Draw;
        }

        private static void Draw(MaterialEditor materialEditor)
        {
            var id = GUIUtility.GetControlID(new GUIContent("Pick shader asset"), FocusType.Passive);

            Material targetMaterial = materialEditor.target as Material;
            Shader shader = targetMaterial.shader;
            if (shader.name.StartsWith("Stubbed"))
            {
                if (GUILayout.Button("Upgrade to Real Shader"))
                {
                    ShaderUpgradeDowngradeHandler.Upgrade((Material)materialEditor.target);
                }
            }

            if (GUILayout.Button("Pick shader asset"))
            {
                EditorGUIUtility.ShowObjectPicker<Shader>(null, false, null, id);
            }

            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == id)
            {
                materialEditor.SetShader(EditorGUIUtility.GetObjectPickerObject() as Shader, true);
            }
        }
    }
}