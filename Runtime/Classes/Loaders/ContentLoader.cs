using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

        public abstract R2APISerializableContentPack SerializableContentPack {get; protected set;}

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

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(ContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
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
                        MSULog.Debug($"This may happen because the required asset has been disabled via config, or because it has the DisabledContent attribute.");
                    }
                }
            }
            if (notAssignedAssets.Count > 0)
            {
                MSULog.Debug($"There where {notAssignedAssets.Count} Assets that have not been assigned to fields inside {typeToPopulate.FullName}, listing assets:");
                notAssignedAssets.ForEach(asset => MSULog.Debug(asset));
            }
        }
    }
}