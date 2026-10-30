using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU
{
    /// <summary>
    /// Class used for finalizing shaders ingame
    /// </summary>
    public static class ShaderUtil
    {
        private static HashSet<Material> _addressableMaterials = new HashSet<Material>();

        [Obsolete("AddressableMaterialShader is obsolete and should not be used. Migrate them to MaterialDefinitions instead.")]
        public static IEnumerator LoadAddressableMaterialShadersAsync(AssetBundle[] assetBundles)
        {
            using ParallelCoroutine helper = ParallelCoroutine.RequestFromPool();

            var list = new List<Material>();
            foreach (var bundle in assetBundles)
            {
                helper.Add(LoadAssetsFromBundle(bundle, list, IsShaderAddressableShader));
            }

            while (!helper.IsDone())
                yield return null;

            var addressableMaterialLoadingRoutine = LoadAddressableMaterialShadersAsync(list);
            while (addressableMaterialLoadingRoutine.MoveNext())
                yield return null;

        }

        [Obsolete("AddressableMaterialShader is obsolete and should not be used. Migrate them to MaterialDefinitions instead.")]
        public static IEnumerator LoadAddressableMaterialShadersAsync(AssetBundle bundle)
        {
            var list = new List<Material>();

            var enumerator = LoadAssetsFromBundle(bundle, list, IsShaderAddressableShader);
            while (enumerator.MoveNext())
                yield return null;

            var addressableMaterialLoadingRoutine = LoadAddressableMaterialShadersAsync(list);
            while (addressableMaterialLoadingRoutine.MoveNext())
                yield return null;
        }

        [Obsolete("AddressableMaterialShader is obsolete and should not be used. Migrate them to MaterialDefinitions instead.")]
        public static IEnumerator LoadAddressableMaterialShadersAsync(List<Material> materials)
        {
            using var helper = ParallelCoroutine.RequestFromPool();

            foreach (var material in materials)
            {
                helper.Add(LoadRealMaterialAndCopyProperties(material));
            }

            while (!helper.IsDone())
                yield return null;

        }

        [Obsolete("AddressableMaterialShader is obsolete and should not be used. Migrate them to MaterialDefinitions instead.")]
        public static IEnumerator LoadAddressableMaterialShadersAsync(Material material) => LoadRealMaterialAndCopyProperties(material);

        /// <summary>
        /// A Coroutine that's used to initialize all <see cref="MaterialDefinition"/>s inside <paramref name="assetBundles"/>, and storing the material instances in a hash set so it doesnt get lost.
        /// </summary>
        /// <param name="bundle">The asset bundles to finalize all of their MaterialDefinitions</param>
        /// <returns>A coroutine which can be yielded or awaited</returns>
        public static IEnumerator InitializeMaterialDefinitionsAsync(AssetBundle[] assetBundles)
        {
            using ParallelCoroutine helper = ParallelCoroutine.RequestFromPool();

            var materialDefinitions = new List<MaterialDefinition>();
            foreach (var bundle in assetBundles)
            {
                helper.Add(LoadAssetsFromBundle(bundle, materialDefinitions, null));
            }

            while (!helper.isDone)
            {
                yield return null;
            }

            var finalizeMaterialDefinitionsAsync = InitializeMaterialDefinitionsAsync(materialDefinitions);
            while (finalizeMaterialDefinitionsAsync.MoveNext())
            {
                yield return null;
            }
        }

        /// <summary>
        /// A Coroutine that's used to initialize all <see cref="MaterialDefinition"/>s inside <paramref name="bundle"/>, and storing the material instances in a hash set so it doesnt get lost.
        /// </summary>
        /// <param name="bundle">The asset bundle to finalize all of it's MaterialDefinitions</param>
        /// <returns>A coroutine which can be yielded or awaited</returns>
        public static IEnumerator InitializeMaterialDefinitionsAsync(AssetBundle bundle)
        {
            List<MaterialDefinition> materialDefinitions = new List<MaterialDefinition>();

            var enumerator = LoadAssetsFromBundle(bundle, materialDefinitions, null);

            while(enumerator.MoveNext())
            {
                yield return null;
            }

            var finalizeMaterialDefinitionsAsync = InitializeMaterialDefinitionsAsync(materialDefinitions);
            while(finalizeMaterialDefinitionsAsync.MoveNext())
            {
                yield return null;
            }
        }

        /// <summary>
        /// A Coroutine that's used to initialize the <paramref name="materialDefinitions"/>, and storing the material instances in a hash set so it doesnt get lost.
        /// </summary>
        /// <param name="materialDefinitions">The material definitions to finalize</param>
        /// <returns>A coroutine which can be yielded or awaited</returns>
        public static IEnumerator InitializeMaterialDefinitionsAsync(List<MaterialDefinition> materialDefinitions)
        {
            using var helper = ParallelCoroutine.RequestFromPool();

            foreach(var material in materialDefinitions)
            {
                helper.Add(InitializeMaterialDefinitionsAsync(material));
            }

            while(!helper.isDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// A Coroutine that's used to initialize the <paramref name="materialDefinition"/>, and storing the material instance in a hash set so it doesnt get lost.
        /// </summary>
        /// <param name="materialDefinition">The material definition to finalize</param>
        /// <returns>A coroutine which can be yielded or awaited</returns>
        public static IEnumerator InitializeMaterialDefinitionsAsync(MaterialDefinition materialDefinition)
        {
            _addressableMaterials.Add(materialDefinition._material);
            return materialDefinition.InitializeDefinition();
        }

        /// <summary>
        /// A Coroutine that's used to swap the stubbed shaders found in the AssetBundles specified in <paramref name="bundles"/>
        /// </summary>
        /// <param name="bundles">The bundles to check</param>
        /// <returns>A Coroutine which can be yielded or awaited</returns>
        public static IEnumerator SwapStubbedShadersAsync(AssetBundle[] bundles)
        {
            using ParallelCoroutine helper = ParallelCoroutine.RequestFromPool();

            List<Material> materials = new List<Material>();
            foreach (var bundle in bundles)
            {
                helper.Add(LoadAssetsFromBundle(bundle, materials, IsShaderStubbedShader));
            }

            while (!helper.IsDone())
                yield return null;

            var enumerator = SwapStubbedShadersAsync(materials);

            while (enumerator.MoveNext())
                yield return null;

        }

        /// <summary>
        /// A Coroutine that's used to swap the stubbed shaders found in the AssetBundle specified in <paramref name="assetBundle"/>
        /// </summary>
        /// <param name="assetBundle">The bundles to check</param>
        /// <returns>A Coroutine which can be yielded or awaited</returns>
        public static IEnumerator SwapStubbedShadersAsync(AssetBundle assetBundle) => ShaderSwapper.ShaderSwapper.UpgradeStubbedShadersAsync(assetBundle);


        /// <summary>
        /// A Coroutine that's used to swap the stubbed shaders found in the materials listed in <paramref name="materials"/>
        /// </summary>
        /// <param name="materials">the materials to check</param>
        /// <returns>A Coroutine which can be yielded or awaited</returns>
        public static IEnumerator SwapStubbedShadersAsync(List<Material> materials)
        {
            using ParallelCoroutine helper = ParallelCoroutine.RequestFromPool();

            foreach (var material in materials)
            {
                helper.Add(ShaderSwapper.ShaderSwapper.UpgradeStubbedShaderAsync(material));
            }

            while (!helper.IsDone())
                yield return null;
        }

        /// <summary>
        /// A Coroutine that's used to swap the stubbed shader of <paramref name="material"/> for it's real counterpart
        /// </summary>
        /// <param name="material">The material to modify</param>
        /// <returns>A Coroutine which can be yielded or awaited</returns>
        public static IEnumerator SwapStubbedShadersAsync(Material material) => ShaderSwapper.ShaderSwapper.UpgradeStubbedShaderAsync(material);

        private static IEnumerator LoadAssetsFromBundle<T>(AssetBundle bundle, List<T> loadedAssets, Func<T, bool> filter = null)
        {
            if (bundle.isStreamedSceneAssetBundle)
                yield break;

            var request = bundle.LoadAllAssetsAsync<T>();
            while (!request.isDone)
                yield return null;

            foreach (var asset in request.allAssets)
            {
                if (asset is T tAsset && (filter?.Invoke(tAsset) ?? true))
                {
                    loadedAssets.Add(tAsset);
                }
            }
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
            while (!asyncOp.IsDone)
                yield return null;

            var loadedMat = asyncOp.Result;
            if (!loadedMat)
            {
                MSULog.Warning($"{material}'s AddressableMaterialShader's address returned a null object. (Address={address})");
                yield break;
            }

            material.shader = loadedMat.shader;
            material.CopyPropertiesFromMaterial(loadedMat);
            material.renderQueue = loadedMat.renderQueue;
            _addressableMaterials.Add(material);
#if DEBUG
            MSULog.Debug($"Properties from {loadedMat} ({address}) copied to {material}");
#endif
        }

        private static bool IsShaderAddressableShader(Material mat)
        {
#if !DEBUG
            return mat.shader.name == "MSU/AddressableMaterialShader";
#endif
            var shaderName = mat.shader.name;

            if (shaderName == "MSU/AddressableMaterialShader")
            {
                return true;
            }
            else if (shaderName == "DEPRECATED/AddressableMaterialShader")
            {
                MSULog.Warning($"Material wtih name {mat.name} has its Shader set to \"DEPRECATED/AddressableMaterialShader\", this shader will be removed in future releases as its been replaced by the shader \"MSU/AddressableMaterialShader\"." +
                    $"\nThis message is purely a warning and the material's addressable representation will be loaded regardless.");
                return true;
            }
            return false;
        }

        private static bool IsShaderStubbedShader(Material mat) => mat.shader.name.StartsWith("Stubbed");
    }
}
