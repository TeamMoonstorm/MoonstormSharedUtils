using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShaderSwapper;
using System.Linq;
using UnityEngine.AddressableAssets;
using System;

namespace MSU
{
    public static class ShaderUtil
    {
        private static HashSet<Material> _addressableMaterials = new HashSet<Material>();

        public static IEnumerator LoadAddressableMaterialShadersAsync(AssetBundle[] assetBundles)
        {
            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();

            var list = new List<Material>();
            foreach (var bundle in assetBundles)
            {
                helper.Add<AssetBundle, List<Material>, Func<Material, bool>>(LoadMaterialsFromBundle, bundle, list, IsShaderAddressableShader);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            var addressableMaterialLoadingRoutine = LoadAddressableMaterialShadersAsync(list);
            while (addressableMaterialLoadingRoutine.MoveNext())
                yield return null;

        }

        public static IEnumerator LoadAddressableMaterialShadersAsync(AssetBundle bundle)
        {
            var list = new List<Material>();

            var enumerator = LoadMaterialsFromBundle(bundle, list, IsShaderAddressableShader);
            while (enumerator.MoveNext())
                yield return null;

            var addressableMaterialLoadingRoutine = LoadAddressableMaterialShadersAsync(list);
            while (addressableMaterialLoadingRoutine.MoveNext())
                yield return null;
        }

        public static IEnumerator LoadAddressableMaterialShadersAsync(List<Material> materials)
        {
            var helper = new ParallelCoroutineHelper();

            foreach (var material in materials)
            {
                helper.Add(LoadRealMaterialAndCopyProperties, material);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;
        }

        public static IEnumerator LoadAddressableMaterialShadersAsync(Material material) => LoadRealMaterialAndCopyProperties(material);

        public static IEnumerator SwapStubbedShadersAsync(AssetBundle[] bundles)
        {
            ParallelCoroutineHelper helper1 = new ParallelCoroutineHelper();

            List<Material> materials = new List<Material>();
            foreach (var bundle in bundles)
            {
                helper1.Add<AssetBundle, List<Material>, Func<Material, bool>>(LoadMaterialsFromBundle, bundle, materials, IsShaderStubbedShader);
            }

            helper1.Start();
            while (!helper1.IsDone())
                yield return null;

            var enumerator = SwapStubbedShadersAsync(materials);

            while (enumerator.MoveNext())
                yield return null;

        }

        public static IEnumerator SwapStubbedShadersAsync(AssetBundle assetBundle) => ShaderSwapper.ShaderSwapper.UpgradeStubbedShadersAsync(assetBundle);

        public static IEnumerator SwapStubbedShadersAsync(List<Material> materials)
        {
            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();

            foreach(var material in materials)
            {
                helper.Add(ShaderSwapper.ShaderSwapper.UpgradeStubbedShaderAsync, material);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;
        }

        public static IEnumerator SwapStubbedShadersAsync(Material material) => ShaderSwapper.ShaderSwapper.UpgradeStubbedShaderAsync(material);

        private static IEnumerator LoadMaterialsFromBundle(AssetBundle bundle, List<Material> materials, Func<Material, bool> filter = null)
        {
            if (bundle.isStreamedSceneAssetBundle)
                yield break;

            var request = bundle.LoadAllAssetsAsync<Material>();
            while (!request.isDone)
                yield return null;

            materials.AddRange(request.allAssets.OfType<Material>().Where(filter));
        }

        private static IEnumerator LoadRealMaterialAndCopyProperties(Material material)
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
            if(!loadedMat)
            {
                MSULog.Warning($"{material}'s AddressableMaterialShader's address returned a null object. (Address={address})");
                yield break;
            }

            material.shader = loadedMat.shader;
            material.CopyPropertiesFromMaterial(material);
            _addressableMaterials.Add(material);
#if DEBUG
            MSULog.Debug($"Properties from {loadedMat} ({address}) copied to {material}");
#endif
        }

        private static bool IsShaderAddressableShader(Material mat) => mat.shader.name == "AddressableMaterialShader";

        private static bool IsShaderStubbedShader(Material mat) => mat.shader.name.StartsWith("Stubbed");
    }
}
