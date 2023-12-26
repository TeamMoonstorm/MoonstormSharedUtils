using BepInEx;
using R2API.ScriptableObjects;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public static class ContentUtil
    {
        public static IContentPieceProvider<T> AnalyzeForContentPieces<T>(BaseUnityPlugin baseUnityPlugin, ContentPack contentPack) where T : UnityEngine.Object
        {
            var assembly = baseUnityPlugin.GetType().Assembly;

            IEnumerable<IContentPiece<T>> contentPieces = assembly.GetTypes()
                .Where(PassesFilter<T>)
                .Select(t => (IContentPiece<T>)Activator.CreateInstance(t));

            return new GenericContentPieceProvider<T>(contentPieces, contentPack);
        }

        public static void AddSingle<T>(this NamedAssetCollection<T> collection, T content) where T : class
        {
            string name = collection.nameProvider(content);
            string backupName = $"{content.GetType().Name}_{collection.Count}";

            bool assetInCollection = collection.assetToName.ContainsKey(content);
            if (assetInCollection)
            {
#if DEBUG
                MSULog.Error($"Cannot add {content} to the NamedAssetCollection of type {typeof(T).Name} because its already in the collection.");
#endif
                return;
            }

            if(name.IsNullOrWhiteSpace())
            {
#if DEBUG
                MSULog.Warning($"Content {content} does not have a valid name! ({name}). assigning a generic name...");
#endif
                name = backupName;
            }

            if(collection.nameToAsset.ContainsKey(name))
            {
#if DEBUG
                MSULog.Warning($"Content {content} cant be addded because an asset with the name \"{name}\" is already registered. Using a generic name.");
                name = backupName;
#endif
            }

            int num = 1;
            int newSize = num + collection.assetInfos.Length;
            HG.ArrayUtils.ArrayAppend(ref collection.assetInfos, new NamedAssetCollection<T>.AssetInfo
            {
                asset = content,
                assetName = name
            });
            collection.nameToAsset[name] = content;
            collection.assetToName[content] = name;

            Array.Sort(collection.assetInfos);
        }

        public static void PopulateTypeFields<TAsset>(Type typeToPopulate, NamedAssetCollection<TAsset> assets) where TAsset : UnityEngine.Object
        {
            MSULog.Info($"Attempting to populate {typeToPopulate.FullName} with {assets.Count} assets");

            List<TAsset> notAssignedAssets = assets.assetInfos.Select(item => item.asset).ToList();
            string[] array = new string[assets.Length];

            StringBuilder failureLog = new StringBuilder();

            for (int i = 0; i < assets.Length; i++)
            {
                array[i] = assets[i].name;
            }

            int missingAssets = 0;
            FieldInfo[] fields = typeToPopulate.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach(FieldInfo fieldInfo in fields)
            {
                if(fieldInfo.FieldType == typeof(TAsset))
                {
                    string name = fieldInfo.Name;
                    TAsset val = assets.Find(name);
                    if(val != null)
                    {
                        notAssignedAssets.Remove(val);
                        fieldInfo.SetValue(null, val);
                        continue;
                    }

                    missingAssets++;
                    failureLog.AppendLine($"Failed to assign {fieldInfo.DeclaringType.FullName}.{fieldInfo.Name}: Asset Not Found.");
                }
            }

            if(failureLog.Length > 1)
            {
                failureLog.Insert(0, $"Failed to assign {missingAssets} field(s), logging which ones have failed.");
                MSULog.Warning(failureLog.ToString());
                failureLog.Clear();
            }

            if(notAssignedAssets.Count > 0)
            {
                failureLog.AppendLine($"There where {notAssignedAssets} Assets that have not been assigned to fields inside {typeToPopulate.FullName}. Listing assets:");
                foreach(var asset in notAssignedAssets)
                {
                    failureLog.AppendLine(asset.name);
                }
                MSULog.Warning(failureLog);
            }
        }

        private static bool PassesFilter<T>(Type t) where T : UnityEngine.Object
        {
            bool notAbstract = !t.IsAbstract;
            bool implementsInterface = t.GetInterfaces().Contains(typeof(IContentPiece<T>));
            return notAbstract && implementsInterface;
        }

        private class GenericContentPieceProvider<T> : IContentPieceProvider<T> where T : UnityEngine.Object
        {
            public ContentPack ContentPack => _contentPack;

            private ContentPack _contentPack;
            private IContentPiece<T>[] _contentPieces;
            public IContentPiece<T>[] GetContents()
            {
                return _contentPieces;
            }

            IContentPiece[] IContentPieceProvider.GetContents()
            {
                return _contentPieces;
            }

            public GenericContentPieceProvider(IEnumerable<IContentPiece<T>> contentPieces, ContentPack contentPack)
            {
                _contentPieces = contentPieces.ToArray();
                _contentPack = contentPack;
            }
        }
    }
}
