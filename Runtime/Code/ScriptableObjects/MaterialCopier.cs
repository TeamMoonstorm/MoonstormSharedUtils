using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm
{
    [CreateAssetMenu(menuName = "Moonstorm/MaterialCopier")]
    public class MaterialCopier : ScriptableObject
    {
        [Serializable]
        public class MaterialPair
        {
            public string materialAddress;
            public Material material;
        }
        private static readonly List<MaterialCopier> instances = new List<MaterialCopier>();
        private static readonly List<Material> copiedMaterials = new List<Material>();

        [SystemInitializer]
        private static void Initialize()
        {
            MSULog.Debug($"Material Copier Initialized");
            foreach(MaterialCopier copier in instances)
            {
                MSULog.Debug($"Copying materials from {copier}");
                copier.CopyMaterials();
            }
        }

        public List<MaterialPair> materialPairs = new List<MaterialPair>();
        public void Awake()
        {
            instances.Add(this);
        }

        public void OnDestroy()
        {
            instances.Remove(this);
        }

        internal void CopyMaterials()
        {
            foreach(MaterialPair pair in materialPairs)
            {
                try
                {
                    MSULog.Debug($"{pair.materialAddress}\n{pair.material}");
                    CopyFromMaterialAddress(pair);
                }
                catch(Exception ex)
                {
                    MSULog.Error(ex);
                }
            }
        }

        private async void CopyFromMaterialAddress(MaterialPair materialPair)
        {
            var asyncOp = Addressables.LoadAssetAsync<Material>(materialPair.materialAddress);
            var task = asyncOp.Task;
            var originalMaterial = await task;
            materialPair.material.shader = originalMaterial.shader;
            materialPair.material.CopyPropertiesFromMaterial(originalMaterial);
            copiedMaterials.Add(materialPair.material);
            MSULog.Debug($"Properties from {originalMaterial} copied to {materialPair.material}");
        }
    }
}
