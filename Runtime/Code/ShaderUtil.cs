using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShaderSwapper;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace MSU
{
    public static class ShaderUtil
    {
        private static HashSet<Material> _addressableMaterials = new HashSet<Material>();
        public static IEnumerator LoadAddressableMaterialShadersAsync(AssetBundle[] assetBundles)
        {
            foreach(AssetBundle bundle in assetBundles)
            {
                yield return LoadAddressableMaterialShadersAsync(bundle);
            }
        }

        public static IEnumerator LoadAddressableMaterialShadersAsync(AssetBundle assetBundle) 
        {
            if (!assetBundle)
                yield break;

            if (assetBundle.isStreamedSceneAssetBundle)
                yield break;

            var request = assetBundle.LoadAllAssetsAsync<Material>();
            while(!request.isDone)
            {
                yield return null;
            }

            yield return LoadAddressableMaterialShadersAsync(request.allAssets.OfType<Material>().ToArray());
        }

        public static IEnumerator LoadAddressableMaterialShadersAsync(Material[] materials)
        {
            foreach(Material material in materials)
            {
                if (material.shader.name != "AddressableMaterialShader")
                    continue;

                yield return LoadAddressableMaterialShadersAsync(material);
            }
        }

        public static IEnumerator LoadAddressableMaterialShadersAsync(Material material)
        {
            var shaderKeywords = material.shaderKeywords;
            if (shaderKeywords.Length == 0)
                yield break;

            var address = shaderKeywords[0];
            if (string.IsNullOrEmpty(address))
                yield break;

            var asyncOp = Addressables.LoadAssetAsync<Material>(address);
            while (asyncOp.IsDone)
                yield return null;

            var loadedMat = asyncOp.Result;
            material.shader = loadedMat.shader;
            material.CopyPropertiesFromMaterial(material);
            _addressableMaterials.Add(material);
#if DEBUG
            MSULog.Debug($"Properties from {loadedMat} ({address}) copied to {material}");
#endif
        }

        public static IEnumerator SwapMaterialShadersAsync(Material[] materials)
        {
            foreach(var material in materials)
            {
                yield return SwapMaterialShadersAsync(material);
            }
        }

        public static IEnumerator SwapMaterialShadersAsync(Material material)
        {
            yield return ShaderSwapper.ShaderSwapper.UpgradeStubbedShaderAsync(material);
        }

        public static IEnumerator SwapAssetBundleShadersAsync(AssetBundle[] bundles)
        {
            foreach(var bundle in bundles)
            {
                yield return SwapAssetBundleShadersAsync(bundle);
            }
        }

        public static IEnumerator SwapAssetBundleShadersAsync(AssetBundle bundle)
        {
            if (bundle.isStreamedSceneAssetBundle)
                yield break;

            yield return ShaderSwapper.ShaderSwapper.UpgradeStubbedShadersAsync(bundle);
        }
    }
}
