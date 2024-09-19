using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moonstorm.Loaders
{
    public abstract class ContentLoader<T> : ContentLoader where T : ContentLoader<T>
    {
        public static T Instance { get; private set; }

        public ContentLoader()
        {
            try
            {
                if (Instance != null)
                {
                    throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting LanguageLoader was instantiated twice");
                }
                Instance = this as T;
            }
            catch (Exception e)
            {
                MSULog.Error(e);
            }
        }
    }

    public abstract class ContentLoader : IContentPackProvider
    {
        public abstract string identifier { get; }

        public ContentPack ContentPack { get; private set; }

        public abstract R2APISerializableContentPack SerializableContentPack { get; protected set; }

        public abstract Action[] LoadDispatchers { get; protected set; }

        public virtual Action[] PopulateFieldsDispatchers { get; protected set; } = Array.Empty<Action>();

        public virtual void Init()
        {
            ContentManager.collectContentPackProviders += AddContent;
        }

        private void AddContent(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public virtual IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public virtual IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(ContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public virtual IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            int j = 0;
            while (j < LoadDispatchers.Length)
            {
                LoadDispatchers[j]();
                args.ReportProgress(Util.Remap(j + 1, 0f, LoadDispatchers.Length, 0f, 0.05f));
                yield return null;
                int num2 = j + 1;
                j = num2;
            }

            EnsureNoFieldsAreNull();
            ContentPack = SerializableContentPack.GetOrCreateContentPack();
            ContentPack.identifier = identifier;

            if (PopulateFieldsDispatchers != null)
            {
                j = 0;
                while (j < PopulateFieldsDispatchers.Length)
                {
                    PopulateFieldsDispatchers[j]();
                    args.ReportProgress(Util.Remap(j + 1, 0f, LoadDispatchers.Length, 0.95f, 0.99f));
                    yield return null;
                    int num2 = j + 1;
                    j = num2;
                }
            }
        }

        private void EnsureNoFieldsAreNull()
        {
            RemoveNullFields(ref SerializableContentPack.artifactDefs);
            RemoveNullFields(ref SerializableContentPack.bodyPrefabs);
            RemoveNullFields(ref SerializableContentPack.buffDefs);
            RemoveNullFields(ref SerializableContentPack.effectPrefabs);
            RemoveNullFields(ref SerializableContentPack.eliteDefs);
            RemoveNullFields(ref SerializableContentPack.entitlementDefs);
            RemoveNullFields(ref SerializableContentPack.entityStateConfigurations);
            RemoveNullFields(ref SerializableContentPack.entityStateTypes);
            RemoveNullFields(ref SerializableContentPack.equipmentDefs);
            RemoveNullFields(ref SerializableContentPack.expansionDefs);
            RemoveNullFields(ref SerializableContentPack.gameEndingDefs);
            RemoveNullFields(ref SerializableContentPack.gameModePrefabs);
            RemoveNullFields(ref SerializableContentPack.itemDefs);
            RemoveNullFields(ref SerializableContentPack.itemRelationshipProviders);
            RemoveNullFields(ref SerializableContentPack.itemRelationshipTypes);
            RemoveNullFields(ref SerializableContentPack.itemTierDefs);
            RemoveNullFields(ref SerializableContentPack.masterPrefabs);
            RemoveNullFields(ref SerializableContentPack.miscPickupDefs);
            RemoveNullFields(ref SerializableContentPack.musicTrackDefs);
            RemoveNullFields(ref SerializableContentPack.networkedObjectPrefabs);
            RemoveNullFields(ref SerializableContentPack.networkSoundEventDefs);
            RemoveNullFields(ref SerializableContentPack.projectilePrefabs);
            RemoveNullFields(ref SerializableContentPack.sceneDefs);
            RemoveNullFields(ref SerializableContentPack.skillDefs);
            RemoveNullFields(ref SerializableContentPack.skillFamilies);
            RemoveNullFields(ref SerializableContentPack.surfaceDefs);
            RemoveNullFields(ref SerializableContentPack.survivorDefs);
            RemoveNullFields(ref SerializableContentPack.unlockableDefs);

            void RemoveNullFields<T>(ref T[] array)
            {
                IEnumerable<T> nonNullValues = array.Where(obj => obj != null);
                array = nonNullValues.ToArray();
            }
        }

        public static void PopulateTypeFields<TAsset>(Type typeToPopulate, NamedAssetCollection<TAsset> assets) where TAsset : UnityEngine.Object
        {
            List<TAsset> notAssignedAssets = assets.assetInfos.Select(asset => asset.asset).ToList();
            string[] array = new string[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                array[i] = assets.GetAssetName(assets[i]);
            }
            FieldInfo[] fields = typeToPopulate.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.FieldType == typeof(TAsset))
                {
                    string name = fieldInfo.Name;
                    TAsset val = assets.Find(name);
                    if (val != null)
                    {
                        notAssignedAssets.Remove(val);
                        fieldInfo.SetValue(null, val);
                        continue;
                    }
                    else
                    {
                        MSULog.Warning($"Failed to assign {fieldInfo.DeclaringType.FullName}.{fieldInfo.Name}: Asset Not Found.");
#if DEBUG
                        MSULog.Debug($"This may happen because the required asset has been disabled via config, or because it has the DisabledContent attribute.");
#endif
                    }
                }
            }
#if DEBUG
            if (notAssignedAssets.Count > 0)
            {
                MSULog.Debug($"There where {notAssignedAssets.Count} Assets that have not been assigned to fields inside {typeToPopulate.FullName}, listing assets:");
                notAssignedAssets.ForEach(asset => MSULog.Debug(asset));
            }
#endif
        }
    }
}