using System.Collections.Generic;
using MSU.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor
{
    public class MaterialVariantAssetPostprocessor : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            List<string> newPaths = new List<string>(paths);
            foreach (string path in paths)
            {
                if(!path.EndsWith(".mat"))
                {
                    continue;
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                foreach(var matVariant in MaterialVariant._instances)
                {
                    if(matVariant.originalMaterial != material)
                    {
                        continue;
                    }

                    if (material.shader.name == "MSU/AddressableMaterialShader")
                        continue;

                    matVariant.ApplyEditor();
                    newPaths.Add(AssetDatabase.GetAssetPath(matVariant._material));
                }
            }
            return newPaths.ToArray();
        }
    }
}