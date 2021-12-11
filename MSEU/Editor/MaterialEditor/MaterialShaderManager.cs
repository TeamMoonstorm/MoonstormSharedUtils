using RoR2EditorKit;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    public class MaterialShaderManager : MonoBehaviour
    {
        public static void Upgrade(Material material)
        {
            var currentShader = material.shader;
            if (ShaderSwapDictionary.stubbedToReal.TryGetValue(currentShader, out Shader realShader))
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
            if (ShaderSwapDictionary.realToStubbed.TryGetValue(currentShader, out Shader realShader))
            {
                if (realShader)
                {
                    material.shader = realShader;
                    Debug.Log($"Succesfully replaced {material.name}'s real shader for the stubbed shader");
                }
            }
        }

        [MenuItem("Tools/MSEU/Upgrade All Shaders")]
        public static void UpgradeAllShaders()
        {
            foreach (Material material in GetAllMaterials(new string[] { "StubbedShader", "StubbedCalmWater", "StubbedDecalicious" }))
            {
                Upgrade(material);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/MSEU/Downgrade All Shaders")]
        public static void DowngradeAllShaders()
        {
            foreach (Material material in GetAllMaterials(new string[] { "Hopoo Games", "CalmWater", "Decalicious" }))
            {
                Downgrade(material);
            }
            AssetDatabase.SaveAssets();
        }

        public static List<Material> GetAllMaterials(string[] shaderNames)
        {
            List<Material> materials = new List<Material>();
            foreach (string name in shaderNames)
            {
                materials = materials.Union(Util.FindAssetsByType<Material>().Where(mat => mat.shader.name.StartsWith(name))).ToList();
            }
            return materials;
        }
    }
}