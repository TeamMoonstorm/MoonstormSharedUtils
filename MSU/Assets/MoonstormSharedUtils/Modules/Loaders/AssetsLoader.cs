using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Loaders
{
    /// <summary>
    /// Class for loading your mod's AssetBundles
    /// <para>Handles swapping stubbed shaders from MoonstormSharedEditorUtils</para>
    /// </summary>
    /// <typeparam name="T">The instance of your class</typeparam>
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
            catch (Exception e)
            {
                MSULog.LogE(e);
            }
        }

        /// <summary>
        /// Loads an asset from the MainAssetbundle, assuming an Instance of AssetLoader exists
        /// </summary>
        /// <typeparam name="TAsset">The type of asset to load</typeparam>
        /// <param name="name">The name of the asset</param>
        /// <returns>The asset</returns>
        public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            if (Instance != null)
            {
                return Instance.MainAssetBundle.LoadAsset<TAsset>(name);
            }
            throw new NullReferenceException("Cannot load asset when there's no instance of AssetLoader!");
        }

        /// <summary>
        /// Loads all assets of type TAsset from the MainAssetBundle, assuming an Instance of AssetLoader exists
        /// </summary>
        /// <typeparam name="TAsset">The Type of asset to load</typeparam>
        /// <returns>An array of TAsset</returns>
        public static TAsset[] LoadAllAssetsOfType<TAsset>() where TAsset : UnityEngine.Object
        {
            if(Instance != null)
            {
                return Instance.MainAssetBundle.LoadAllAssets<TAsset>();
            }
            throw new NullReferenceException("Cannot load asset when there's no instance of AssetLoader!");
        }
    }
    /// <summary>
    /// Class for loading your mod's AssetBundles
    /// <para>Handles swapping stubbed shaders from MoonstormSharedEditorUtils</para>
    /// <para>Inherit from AssetsLoaderT instead</para>
    public abstract class AssetsLoader
    {
        /// <summary>
        /// Your mod's Main AssetBundle
        /// </summary>
        public abstract AssetBundle MainAssetBundle { get; }

        /// <summary>
        /// The directory of your assembly
        /// </summary>
        public abstract string AssemblyDir { get; }

        /// <summary>
        /// List holding all the materials with swapped shaders
        /// </summary>
        public static List<Material> MaterialsWithSwappedShaders { get; private set; } = new List<Material>();

        /// <summary>
        /// Automatically loads all the EffectDefs from your assetbundle using the EffectDefHolder
        /// </summary>
        /// <param name="bundle">The bundle to load from</param>
        /// <returns>An array of all the EffectDefs</returns>
        protected EffectDef[] LoadEffectDefsFromHolders(AssetBundle bundle)
        {
            EffectDefHolder[] effectDefHolders = bundle.LoadAllAssets<EffectDefHolder>();

            List<EffectDef> effects = new List<EffectDef>();

            for(int i = 0; i < effectDefHolders.Length; i++)
            {
                EffectDefHolder currentHolder = effectDefHolders[i];

                effects.AddRange(currentHolder.ToEffectDefs());
            }

            return effects.ToArray();
        }

        /// <summary>
        /// Automatically loads all the EffectDefs from your assetbundle by looking for prefabs with the EffectComponent component
        /// </summary>
        /// <param name="bundle">The bundle to load from</param>
        /// <returns>An array of all the EffectDefs</returns>
        protected EffectDef[] LoadEffectDefsFromPrefabs(AssetBundle bundle)
        {
            GameObject[] goWithComponents = bundle.LoadAllAssets<GameObject>().Where(go => go.GetComponent<EffectComponent>()).ToArray();

            return goWithComponents.Select(go => new EffectDef(go)).ToArray();
        }

        /// <summary>
        /// Adds all the given EffectDefs to a SerializableContentPack
        /// </summary>
        /// <param name="effectDefs">The effectDefs to add</param>
        /// <param name="contentPack">The SerializableContentPack to be filled with effectDefs</param>
        public void AddEffectDefsToSerializableContentPack(EffectDef[] effectDefs, SerializableContentPack contentPack)
        {
            effectDefs.ToList().ForEach(ed => HG.ArrayUtils.ArrayAppend(ref contentPack.effectDefs, ed));
        }

        /// <summary>
        /// Adds all the given EffectDefs to a ContentPack
        /// </summary>
        /// <param name="effectDefs">The EffectDefs to add</param>
        /// <param name="contentPack">The ContentPack to be filled with EffectDefs</param>
        public void AddEffectDefsToContentPack(EffectDef[] effectDefs, ContentPack contentPack)
        {
            contentPack.effectDefs.Add(effectDefs);
        }

        /// <summary>
        /// Swaps all the stubbed shaders from MoonstormSharedEditorUtils to use the correct shaders
        /// <para>Automatically stores the swapped shaders in a static list</para>
        /// </summary>
        /// <param name="bundle">The bundle to load all the materials with shaders to be swapped</param>
        protected void SwapShadersFromMaterialsInBundle(AssetBundle bundle)
        { 
            if(bundle.isStreamedSceneAssetBundle)
            {
                MSULog.LogW($"Cannot map materials from a streamed scene asset bundle.");
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
            }
        }
    }
}