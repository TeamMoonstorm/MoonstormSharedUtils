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
            /*if(bundle.isStreamedSceneAssetBundle)
            {
                MSULog.Warning($"Cannot map materials from a streamed scene asset bundle.");
                return;
            }

            var cloudMat = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect").transform.Find("Ring").GetComponent<ParticleSystemRenderer>().material;

            Material[] assetBundleMaterials = bundle.LoadAllAssets<Material>();

            Material[] gameMaterials = Resources.FindObjectsOfTypeAll<Material>();

            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                var material = assetBundleMaterials[i];
                if (material.shader.name.StartsWith("StubbedCalmWater"))
                {
                    material.shader = Shader.Find(material.shader.name.Substring(7));
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }
                if (material.shader.name.StartsWith("StubbedDecalicious"))
                {
                    material.shader = Shader.Find(material.shader.name.Substring(8));
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }
                // If it's stubbed, just switch out the shader unless it's fucking cloudremap
                if (material.shader.name.StartsWith("StubbedShader"))
                {
                    material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
                    if (material.shader.name.Contains("Cloud Remap"))
                    {
                        var eatShit = new RuntimeCloudMaterialMapper(material);
                        material.CopyPropertiesFromMaterial(cloudMat);
                        eatShit.SetMaterialValues(ref material);
                    }
                    MaterialsWithSwappedShaders.Add(material);
                    continue;
                }

                //If it's this shader it searches for a material with the same name and copies the properties
                if (material.shader.name.Equals("CopyFromRoR2"))
                {
                    foreach (var gameMaterial in gameMaterials)
                        if (material.name.Equals(gameMaterial.name))
                        {
                            material.shader = gameMaterial.shader;
                            material.CopyPropertiesFromMaterial(gameMaterial);
                            MaterialsWithSwappedShaders.Add(material);
                            break;
                        }
                    continue;
                }
            }*/
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