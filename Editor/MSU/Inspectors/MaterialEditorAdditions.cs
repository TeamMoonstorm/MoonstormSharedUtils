using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU.Editor.Inspectors
{
    [InitializeOnLoad]
    public static class MaterialEditorAdditions
    {
        public static event Action<MaterialEditor> onDraw;
        public static HashSet<int> _objectInstances = new HashSet<int>();
        static MaterialEditorAdditions()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += Draw;
        }

        private static void Draw(UnityEditor.Editor obj)
        {
            if(obj is not MaterialEditor materialEditor)
            {
                _objectInstances.Clear();
                return;
            }

            Material targetMaterial = materialEditor.target as Material;

            if (targetMaterial.hideFlags.HasFlag(HideFlags.NotEditable))
            {
                if(_objectInstances.Contains(targetMaterial.GetInstanceID()))
                {
                    return;
                }

                _objectInstances.Add(targetMaterial.GetInstanceID());
                if(AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GetAssetPath(targetMaterial)) == typeof(MaterialVariant))
                {
                    AssetDatabase.LoadAssetAtPath<MaterialVariant>(AssetDatabase.GetAssetPath(targetMaterial)).ApplyEditor();
                }
                return;
            }

            Shader shader = targetMaterial.shader;

            if (shader.name == "MSU/AddressableMaterialShader")
            {
                AddressableMaterialShaderHeader(obj);
            }
            if (shader.name == "DEPRECATED/AddressableMaterialShader")
            {
                UpgradeAddressableMaterialShader(obj);
            }
            onDraw?.Invoke(materialEditor);
        }

        private static void AddressableMaterialShaderHeader(UnityEditor.Editor obj)
        {
            ShowAboutLabel();
            SerializedObject so = obj.serializedObject;
            SerializedProperty shaderKeywords = so.FindProperty("m_InvalidKeywords");
            shaderKeywords.arraySize = 2;
            if (shaderKeywords.GetArrayElementAtIndex(0).stringValue == shaderKeywords.GetArrayElementAtIndex(1).stringValue)
            {
                shaderKeywords.GetArrayElementAtIndex(1).stringValue = string.Empty;
            }

            EditorGUI.BeginChangeCheck();
            var addressKeyword = shaderKeywords.GetArrayElementAtIndex(0);
            addressKeyword.stringValue = EditorGUILayout.DelayedTextField(new GUIContent("Address"), addressKeyword.stringValue);
            var addressKeywordStringValue = addressKeyword.stringValue;
            if (EditorGUI.EndChangeCheck() || shaderKeywords.GetArrayElementAtIndex(1).stringValue.IsNullOrEmptyOrWhiteSpace())
            {
                var stubbedShaderKeyword = shaderKeywords.GetArrayElementAtIndex(1);

                var resourceLocations = Addressables.LoadResourceLocationsAsync(addressKeyword.stringValue).WaitForCompletion();
                if(!resourceLocations.Any())
                    return;
                    
                var mat = Addressables.LoadAssetAsync<Material>(addressKeyword.stringValue).WaitForCompletion();
                if (mat && ShaderDictionary.addressableShaderNameToStubbed.TryGetValue(mat.shader.name, out var stubbed))
                {
                    stubbedShaderKeyword.stringValue = stubbed.name;
                }
                else
                {
                    shaderKeywords.arraySize = 1;
                    shaderKeywords.GetArrayElementAtIndex(0).stringValue = addressKeywordStringValue;
                    EditorGUILayout.HelpBox("ShaderDictionary is not populated, please populate with at least the stubbed shaders.", MessageType.Warning, true);
                    MSULog.Warning($"Shader Dictionary is not populated, please populate with at least stubbed shaders");
                }
            }
            so.ApplyModifiedProperties();
        }

        private static void UpgradeAddressableMaterialShader(UnityEditor.Editor obj)
        {
            EditorGUILayout.LabelField(new GUIContent("WARNING (Hover Me.)", "This AddressableMaterialShader is deprecated, click the button below to update"));

            if (GUILayout.Button("Upgrade"))
            {
                SerializedObject so = obj.serializedObject;
                SerializedProperty sp = so.FindProperty("m_Shader");

                sp.objectReferenceValue = Shader.Find("MSU/AddressableMaterialShader");
                so.ApplyModifiedProperties();
            }
        }

        private static void ShowAboutLabel()
        {
            EditorGUILayout.LabelField(new GUIContent("About the AddressableMaterialShader (Hover me!)", "The AddressableMaterialShader is a custom addressable material solution from MSU, it stores the Addressable Material's address in the \"Address\" field.\n" +
                "Later at Runtime, calling your AssetsLoader's FinalizeMaterialsWithAddressableMaterialShader() method will copy the properties and shader of the addressable material to this isntance, effectively allowing you to reference it ingame."));
        }
    }
}
