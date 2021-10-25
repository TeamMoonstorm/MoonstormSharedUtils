using UnityEngine;

namespace Moonstorm.EditorUtils
{
    public class MaterialShaderUpgrader : MonoBehaviour
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
    }
}