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
    public abstract class AssetsLoader<T> : AssetsLoader where T : AssetsLoader<T>
    {
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

        public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            if (Instance != null)
            {
                return Instance.MainAssetBundle.LoadAsset<TAsset>(name);
            }
            MSULog.Error("Cannot load asset when there's no instance of AssetLoader!");
            return null;
        }

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
    public abstract class AssetsLoader
    {
        public abstract AssetBundle MainAssetBundle { get; }

        public static List<Material> MaterialsWithSwappedShaders { get; } = new List<Material>();

        protected void SwapShadersFromMaterialsInBundle(AssetBundle bundle)
        { 
            if(bundle.isStreamedSceneAssetBundle)
            {
                MSULog.Warning($"Cannot swap material shaders from a streamed scene assetbundle.");
                return;
            }

            Material[] assetBundleMaterials = bundle.LoadAllAssets<Material>();

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