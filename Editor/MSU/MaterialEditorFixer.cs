using RoR2.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor
{
    [CreateAssetMenu(menuName = "MSU/Editor/MaterialEditorFixer")]
    public class MaterialEditorFixer : ScriptableObject
    {
        [Serializable]
        public struct MaterialToShaderName
        {
            public Material materialAsset;
            public string shaderName;
        }

        public MaterialToShaderName[] materialsToFix;

        public void SearchForMaterialsAndSaveMetadata()
        {
            Shader[] stubbedShaders = GetStubbedShaders();

            using var progressBar = new DisposableProgressBar("Material Fixer Searching", "Searching for Materials with stubbed shaders", 0);

            string[] guids = AssetDatabase.FindAssets("t:material");
            List<Material> foundMaterials = new List<Material>();
            for(int i = 0; i < guids.Length; i++)
            {
                progressBar.Update(R2EKMath.Remap(i, 0, guids.Length, 0, 0.5f), null, $"Searching for materials with stubbed shaders ({i + 1}/{guids.Length})");
                foundMaterials.Add(AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[i])));
            }

            List<MaterialToShaderName> materialToShaderNames = new List<MaterialToShaderName>();
            for(int i = 0; i < foundMaterials.Count; i++)
            {
                progressBar.Update(R2EKMath.Remap(i, 0, foundMaterials.Count, 0, 1f), null, $"Processing material {i + 1} out of {foundMaterials.Count}");
                var material = foundMaterials[i];

                if(stubbedShaders.Contains(material.shader))
                {
                    materialToShaderNames.Add(new MaterialToShaderName
                    {
                        materialAsset = material,
                        shaderName = material.shader.name
                    });
                }
            }

            materialsToFix = materialToShaderNames.ToArray();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            MSULog.Info($"Saved a total of {materialToShaderNames.Count} materials to fix in MSU 3.0");
        }

        private Shader[] GetStubbedShaders()
        {
            string rootPath = AssetDatabase.GUIDToAssetPath("9baa48c4908f85f43ae0c54e90e44447");
            string directory = System.IO.Path.GetDirectoryName(rootPath);
            string folderToSearch = RoR2.Editor.IOUtils.FormatPathForUnity(directory);
            string[] guids = AssetDatabase.FindAssets("t:Shader", new string[] { folderToSearch });

            List<Shader> stubbedShaders = new List<Shader>();
            for(int i = 0; i < guids.Length; i++)
            {
                stubbedShaders.Add(AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guids[i])));
            }

            return stubbedShaders.ToArray();
        }

        [CustomEditor(typeof(MaterialEditorFixer))]
        private class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginDisabledGroup(true);
                DrawDefaultInspector();
                EditorGUI.EndDisabledGroup();

                if(GUILayout.Button("Find materials to fix") && target is MaterialEditorFixer instance)
                {
                    instance.SearchForMaterialsAndSaveMetadata();
                }
            }
        }
    }
}
