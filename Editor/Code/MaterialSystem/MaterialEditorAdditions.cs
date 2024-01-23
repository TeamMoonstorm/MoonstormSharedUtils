using UnityEditor;
using UnityEngine;

namespace MSU.Editor.ShaderSystem
{
    [InitializeOnLoad]
    public class MaterialEditorAdditions
    {
        static MaterialEditorAdditions()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += Draw;
        }

        private static void Draw(UnityEditor.Editor obj)
        {
            if (!(obj is MaterialEditor materialEditor))
            {
                return;
            }

            var id = GUIUtility.GetControlID(new GUIContent("Pick shader asset"), FocusType.Passive);

            Material targetMaterial = materialEditor.target as Material;
            Shader shader = targetMaterial.shader;
            if (shader.name.StartsWith("Stubbed"))
            {
                if (GUILayout.Button("Upgrade to Real Shader"))
                {
                    MaterialShaderManager.Upgrade((Material)materialEditor.target);
                }
            }
            if (shader.name == "AddressableMaterialShader")
            {
                AddressableMaterialShaderHeader(obj);
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

        private static void AddressableMaterialShaderHeader(UnityEditor.Editor obj)
        {
            ShowAboutLabel();
            SerializedObject so = obj.serializedObject;
            SerializedProperty shaderKeywords = so.FindProperty("m_ShaderKeywords");
            shaderKeywords.stringValue = EditorGUILayout.TextField(new GUIContent("Address"), shaderKeywords.stringValue);
            so.ApplyModifiedProperties();
        }

        private static void ShowAboutLabel()
        {
            EditorGUILayout.LabelField(new GUIContent("About the AddressableMaterialShader (Hover me!)", "The AddressableMaterialShader is a custom addressable material solution from MSU, it stores the Addressable Material's address in the \"Address\" field.\n" +
                "Later at Runtime, calling your AssetsLoader's FinalizeMaterialsWithAddressableMaterialShader() method will copy the properties and shader of the addressable material to this isntance, effectively allowing you to reference it ingame."));
        }
    }
}