using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor
{
    public static class MaterialShaderManager
    {
        public static void Upgrade(Material material)
        {
            var currentShader = material.shader;
            if (ShaderDictionary.hlslToYaml.TryGetValue(currentShader, out Shader realShader))
            {
                if (realShader)
                {
                    material.shader = realShader;
                    Debug.Log($"Succesfully replaced {material.name}'s stubbed shader for the real shader");
                }
            }
        }

        public static void Downgrade(Material material)
        {
            var currentShader = material.shader;
            if (ShaderDictionary.yamlToHlsl.TryGetValue(currentShader, out Shader stubbedShader))
            {
                if (stubbedShader)
                {
                    material.shader = stubbedShader;
                    Debug.Log($"Succesfully replaced {material.name}'s real shader for the stubbed shader");
                }
            }
        }

        [MenuItem(MSUConstants.MSU_MENU_ROOT + "Shaders/Upgrade All")]
        public static void UpgradeAllShaders()
        {
            foreach (Material material in GetAllMaterials())
            {
                Upgrade(material);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem(MSUConstants.MSU_MENU_ROOT + "Shaders/Downgrade All")]
        public static void DowngradeAllShaders()
        {
            foreach (Material material in GetAllMaterials())
            {
                Downgrade(material);
            }
            AssetDatabase.SaveAssets();
        }

        public static List<Material> GetAllMaterials()
        {
            return RoR2.Editor.AssetDatabaseUtil.FindAssetsByType<Material>().Where(mat => ShaderDictionary.instance.GetAllShadersFromDictionary().Contains(mat.shader)).ToList();
        }
    }
}