using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Loaders
{
    /// <summary>
    /// The AssetsLoader is a class that can be used to simplify the loading of assetbundles
    /// <para>AssetsLoaders can be used to easily swap the material's stubbed shaders for the real ones</para>
    /// <para>AssetLoader inheriting classes are treated as Singletons</para>
    /// </summary>
    /// <typeparam name="T">The class that's inheriting from AssetsLoader</typeparam>
    public abstract class AssetsLoader<T> : AssetsLoader where T : AssetsLoader<T>
    {
        /// <summary>
        /// Retrieves the instance of <typeparamref name="T"/>
        /// </summary>
        public static T Instance { get; private set; }

        public AssetsLoader()
        {
            try
            {
                if (Instance != null)
                {
                    throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting AssetsLoader was instantiated twice");
                }
                Instance = this as T;
            }
            catch (Exception e) { MSULog.Error(e); }
        }

        /// <summary>
        /// Loads an asset from the AssetsLoader's MainAssetBundle
        /// Requires an instance to exist.
        /// </summary>
        public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            if (Instance != null)
            {
                return Instance.MainAssetBundle.LoadAsset<TAsset>(name);
            }
            MSULog.Error("Cannot load asset when there's no instance of AssetLoader!");
            return null;
        }

        /// <summary>
        /// Loads an asset of type <typeparamref name="TAsset"/> from the AssetsLoader's MainAssetBundle
        /// Requires an instance to exist.
        /// </summary>
        public static TAsset[] LoadAllAssetsOfType<TAsset>() where TAsset : UnityEngine.Object
        {
            if (Instance != null)
            {
                return Instance.MainAssetBundle.LoadAllAssets<TAsset>();
            }
            MSULog.Error("Cannot load assets when there's no instance of AssetLoader!");
            return null;
        }
    }
    /// <summary>
    /// <inheritdoc cref="AssetsLoader{T}"/>
    /// <para>You probably want to use <see cref="AssetsLoader"/> instead</para>
    /// </summary>
    public abstract class AssetsLoader
    {
        /// <summary>
        /// Your mod's main assetbundle
        /// </summary>
        public abstract AssetBundle MainAssetBundle { get; }

        /// <summary>
        /// A list of all materials that have swapped shaders
        /// </summary>
        public static List<Material> MaterialsWithSwappedShaders { get; } = new List<Material>();

        /// <summary>
        /// Swaps the shaders from all the materials insided the specified bundle
        /// </summary>
        /// <param name="bundle">The bundle where the materials will be loaded from</param>
        protected void SwapShadersFromMaterialsInBundle(AssetBundle bundle)
        { 
            if(bundle.isStreamedSceneAssetBundle)
            {
                MSULog.Warning($"Cannot swap material shaders from a streamed scene assetbundle.");
                return;
            }

            Material[] assetBundleMaterials = bundle.LoadAllAssets<Material>().Where(mat => mat.shader.name.StartsWith("Stubbed")).ToArray();

            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                var material = assetBundleMaterials[i];
                if(!material.shader.name.StartsWith("Stubbed"))
                {
                    MSULog.Warning($"The material {material} has a shader which's name doesnt start with \"Stubbed\" ({material.shader.name}), this is not allowed for stubbed shaders for MSU. not swapping shader.");
                    continue;
                }
                try
                {
                    SwapShader(material);
                }
                catch(Exception ex)
                {
                    MSULog.Error($"Failed to swap shader of material {material}: {ex}");
                }
            }
        }

        /// <summary>
        /// Swaps the shaders from the specified materials in <paramref name="materials"/>
        /// </summary>
        /// <param name="materials">The materials to modify</param>
        protected void SwapShadersFromMaterials(IEnumerable<Material> materials)
        { 
            foreach(Material material in materials)
            {
                if(!material.shader.name.StartsWith("Stubbed"))
                {
                    MSULog.Warning($"The material {material} has a shader which's name doesnt start with \"Stubbed\" ({material.shader.name}), this is not allowed for stubbed shaders for MSU. not swapping shader.");
                    continue;
                }
                try
                {
                    SwapShader(material);
                }
                catch (Exception ex)
                {
                    MSULog.Error($"Failed to swap shader of material {material}: {ex}");
                }
            }
        }

        /// <summary>
        /// Loads all the Materials with the AddressableMaterialShader shader and finalizes them
        /// </summary>
        /// <param name="bundle">The bundle to use for loading the materials</param>
        protected async void FinalizeMaterialsWithAddressableMaterialShader(AssetBundle bundle)
        {
            var materials = bundle.LoadAllAssets<Material>().Where(m => m.shader.name == "AddressableMaterialShader");
            foreach(Material material in materials)
            {
                try
                {
                    var shaderKeywords = material.shaderKeywords;
                    var address = shaderKeywords[0];
                    if (string.IsNullOrEmpty(address))
                        continue;

                    var asyncOp = Addressables.LoadAssetAsync<Material>(address);
                    var loadedMat = await asyncOp.Task;

                    material.shader = loadedMat.shader;
                    material.CopyPropertiesFromMaterial(loadedMat);
                    MaterialsWithSwappedShaders.Add(material);
                    MSULog.Debug($"Properties from {loadedMat} ({address}) copied to {material}");
                }
                catch(Exception e)
                {
                    MSULog.Error($"Failed to finalize material {material}: {e}");
                }
            }
        }

        private async void SwapShader(Material material)
        {
            var shaderName = material.shader.name.Substring("Stubbed".Length);
            var adressablePath = $"{shaderName}.shader";
            var asyncOp = Addressables.LoadAssetAsync<Shader>(adressablePath);
            var shaderTask = asyncOp.Task;
            var shader = await shaderTask;
            material.shader = shader;
            if (material.shader.name.Contains("Cloud Remap"))
            {
                var cloudMatAsyncOp = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matLightningLongBlue.mat");
                var cloudMat = await cloudMatAsyncOp.Task;
                var remapper = new RuntimeCloudMaterialMapper(material);
                material.CopyPropertiesFromMaterial(cloudMat);
                remapper.SetMaterialValues(ref material);
            }
            MaterialsWithSwappedShaders.Add(material);
        }
    }
}