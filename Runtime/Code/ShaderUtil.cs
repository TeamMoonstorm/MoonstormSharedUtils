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
            ParallelCoroutineHelper parallelCoroutineHelper = new ParallelCoroutineHelper();
            foreach(AssetBundle bundle in assetBundles)
            {
                parallelCoroutineHelper.Add(LoadAddressableMaterialShadersAsync, bundle);
            }
            parallelCoroutineHelper.Start();
            while (!parallelCoroutineHelper.IsDone())
                yield return null;
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

            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();
            foreach(var material in request.allAssets.OfType<Material>())
            {
                helper.Add(LoadAddressableMaterialShadersAsync, material);
            }

            helper.Start();
            while(!helper.IsDone()) yield return null;
        }

        public static IEnumerator LoadAddressableMaterialShadersAsync(Material[] materials)
        {
            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();
            foreach(Material material in materials)
            {
                if (material.shader.name != "AddressableMaterialShader")
                    continue;

                helper.Add(LoadAddressableMaterialShadersAsync, material);
            }

            helper.Start();
            while (!helper.IsDone()) yield return null;
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
            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();
            foreach(var material in materials)
            {
                helper.Add(SwapMaterialShadersAsync, material);
            }
            helper.Start();
            while (!helper.IsDone()) yield return null;
        }

        public static IEnumerator SwapMaterialShadersAsync(Material material)
        {
            return ShaderSwapper.ShaderSwapper.UpgradeStubbedShaderAsync(material);
        }

        public static IEnumerator SwapAssetBundleShadersAsync(AssetBundle[] bundles)
        {
            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();
            foreach (var bundle in bundles)
            {
                helper.Add(SwapAssetBundleShadersAsync, bundle);
            }
            helper.Start();
            while (!helper.IsDone())
                yield return null;
        }

        public static IEnumerator SwapAssetBundleShadersAsync(AssetBundle bundle)
        {
            if (bundle.isStreamedSceneAssetBundle)
                yield break;

            var enumerator = ShaderSwapper.ShaderSwapper.UpgradeStubbedShadersAsync(bundle);
            while (enumerator.MoveNext())
                yield return null;
        }
    }
}
