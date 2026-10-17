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

        public void FixMaterials()
        {
            using var _0 = new AssetDatabaseUtil.AssetEditingSaveScope();
            using var _ = HG.StringBuilderPool.RentStringBuilder(out var sb);

            //Get the stubbed shaders, have a list of materials that need to be made dirty
            Shader[] stubbedShaders = GetStubbedShaders();
            List<Material> dirtyMaterials = new List<Material>();

            //Iterate backwards
            for (int i = materialsToFix.Length - 1; i >= 0; i--)
            {
                var materialToFix = materialsToFix[i];

                //Get material asset and the stubbed shader name to find
                var materialAsset = materialToFix.materialAsset;
                var stubbedShaderName = materialToFix.shaderName;

                //Find the shader
                Shader shaderMatch = null;
                for(int j = 0; j < stubbedShaders.Length; j++)
                {
                    Shader potentialShader = stubbedShaders[j];
                    if(string.Equals(potentialShader.name, stubbedShaderName))
                    {
                        shaderMatch = potentialShader;
                        break;
                    }
                }

                //Found a match, fix it, remove from the materialsToFix array, and add it to the dirty materials list
                if(shaderMatch)
                {
                    sb.AppendLine($"Fixing {materialAsset}, assigning {shaderMatch}");
                    
                    materialAsset.shader = shaderMatch;
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref materialsToFix, i);

                    dirtyMaterials.Add(materialAsset);
                }
                else
                { //No match found, log
                    sb.AppendLine($"Warn: Unable to fix {materialAsset}, it's stored shader name is {stubbedShaderName} yet there's no stubbed shader with that name.");
                }
            }

            //Set dirty
            foreach(var material in dirtyMaterials)
            {
                EditorUtility.SetDirty(material);
            }

            EditorUtility.SetDirty(this);

            //Log
            MSULog.Info(sb.ToString());
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

                if(GUILayout.Button("Fix Materials") && target is MaterialEditorFixer instance)
                {
                    instance.FixMaterials();
                    if(instance.materialsToFix.Length > 0)
                    {
                        EditorUtility.DisplayDialog("Issue with MaterialEditorFixer", $"It seems like {instance.materialsToFix.Length} materials where unable to be fixed. The array of materials to fix now contains materials that don't have stubbed shaders assigned.", "Ok");
                    }
                }
            }
        }
    }
}
