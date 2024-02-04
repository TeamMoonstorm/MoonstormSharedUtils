using BepInEx;
using RoR2.Skills;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ExpansionManagement;
using R2API.Utils;
using RoR2.EntitlementManagement;
using UnityEngine.Networking;
using RoR2.Projectile;

namespace MSU
{
    public static class ContentUtil
    {
        //Dummy expansion def, we dont add it to the Catalog to make it so stuff thats disabled cant be added.
        private static ExpansionDef _dummyExpansion = ScriptableObject.CreateInstance<ExpansionDef>();
        public static void DisableArtifact(ArtifactDef artifactDef)
        {
            //setting expansion to an invalid value makes it impossible for it to appear in lobby.
            artifactDef.requiredExpansion = _dummyExpansion;
        }
        
        public static void DisableSurvivor(SurvivorDef survivorDef)
        {
            survivorDef.hidden = true;
        }

        public static void DisableItem(ItemDef itemDef)
        {
            itemDef.requiredExpansion = _dummyExpansion;
        }

        public static void DisableEquipment(EquipmentDef equipmentDef)
        {
            equipmentDef.requiredExpansion = _dummyExpansion;
        }

        public static IContentPieceProvider<T> AnalyzeForContentPieces<T>(BaseUnityPlugin baseUnityPlugin, ContentPack contentPack) where T : UnityEngine.Object
        {
            var assembly = baseUnityPlugin.GetType().Assembly;

            IEnumerable<IContentPiece<T>> contentPieces = ReflectionCache.GetTypes(assembly)
                .Where(PassesFilter<T>)
                .Select(t => (IContentPiece<T>)Activator.CreateInstance(t));

            return new GenericContentPieceProvider<T>(contentPieces, contentPack);
        }

        public static IContentPieceProvider<GameObject> AnalyzeForGameObjectContentPieces<T>(BaseUnityPlugin baseUnityPlugin, ContentPack contentPack)
        {
            var assembly = baseUnityPlugin.GetType().Assembly;
            
            IEnumerable<IContentPiece<GameObject>> contentPieces = ReflectionCache.GetTypes(assembly)
                .Where(t => PassesFilter<GameObject>(t) && t.GetInterfaces().Contains(typeof(IGameObjectContentPiece<T>)))
                .Select(t => (IContentPiece<GameObject>)Activator.CreateInstance(t));

            return new GenericContentPieceProvider<GameObject>(contentPieces, contentPack);
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
                MSULog.Warning($"Content {content} cant be added because an asset with the name \"{name}\" is already registered. Using a generic name.");
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

        public static void AddContentFromAssetCollection(this ContentPack contentPack, AssetCollection collection)
        {
            foreach(var asset in collection.assets)
            {
                try
                {
                    if (!asset)
                        continue;

                    HandleAssetAddition(asset, contentPack);
                }
                catch(Exception e)
                {
                    MSULog.Error(e);
                }
            }
        }

        private static void HandleAssetAddition(UnityEngine.Object asset, ContentPack contentPack)
        {
            switch (asset)
            {
                case UnityEngine.GameObject gObject: HandleGameObjectAddition(gObject, contentPack); break;
                case SkillDef sd: contentPack.skillDefs.AddSingle(sd); break;
                case SkillFamily sf: contentPack.skillFamilies.AddSingle(sf); break;
                case SceneDef sd: contentPack.sceneDefs.AddSingle(sd); break;
                case ItemDef id: contentPack.itemDefs.AddSingle(id); break;
                case ItemTierDef itd: contentPack.itemTierDefs.AddSingle(itd); break;
                case ItemRelationshipProvider irp: contentPack.itemRelationshipProviders.AddSingle(irp); break;
                case ItemRelationshipType irt: contentPack.itemRelationshipTypes.AddSingle(irt); break;
                case EquipmentDef ed: contentPack.equipmentDefs.AddSingle(ed); break;
                case BuffDef bd: contentPack.buffDefs.AddSingle(bd); break;
                case EliteDef _ed: contentPack.eliteDefs.AddSingle(_ed); break;
                case UnlockableDef ud: contentPack.unlockableDefs.AddSingle(ud); break;
                case SurvivorDef _sd: contentPack.survivorDefs.AddSingle(_sd); break;
                case ArtifactDef ad: contentPack.artifactDefs.AddSingle(ad); break;
                case SurfaceDef __sd: contentPack.surfaceDefs.AddSingle(__sd); break;
                case NetworkSoundEventDef nsed: contentPack.networkSoundEventDefs.AddSingle(nsed); break;
                case MusicTrackDef mtd: contentPack.musicTrackDefs.AddSingle(mtd); break;
                case GameEndingDef ged: contentPack.gameEndingDefs.AddSingle(ged); break;
                case EntityStateConfiguration esc: contentPack.entityStateConfigurations.AddSingle(esc); break;
                case ExpansionDef __ed: contentPack.expansionDefs.AddSingle(__ed); break;
                case EntitlementDef ___ed: contentPack.entitlementDefs.AddSingle(___ed); break;
                case MiscPickupDef mpd: contentPack.miscPickupDefs.AddSingle(mpd); break;
                case EntityStateTypeCollection estc: AddEntityStateTypes(estc, contentPack); break;
            }
        }

        private static void HandleGameObjectAddition(GameObject go, ContentPack contentPack)
        {
            NetworkIdentity identity = go.GetComponent<NetworkIdentity>();
            bool isNetworkedByDefault = false;
            if(go.TryGetComponent<CharacterBody>(out var bodyComponent))
            {
                isNetworkedByDefault = true;
                contentPack.bodyPrefabs.AddSingle(go);
            }
            if(go.TryGetComponent<CharacterMaster>(out var masterComponent))
            {
                isNetworkedByDefault = true;
                contentPack.masterPrefabs.AddSingle(go);
            }
            if(go.TryGetComponent<ProjectileController>(out var controllerComponent))
            {
                isNetworkedByDefault = true;
                contentPack.projectilePrefabs.AddSingle(go);
            }
            if(go.TryGetComponent<Run>(out var runComponent))
            {
                isNetworkedByDefault = true;
                contentPack.gameModePrefabs.AddSingle(go);
            }
            if(go.TryGetComponent<EffectComponent>(out var effectComponent))
            {
                contentPack.effectDefs.AddSingle(new EffectDef(go));
            }

            if(identity && !isNetworkedByDefault)
            {
                contentPack.networkedObjectPrefabs.AddSingle(go);
            }
        }

        private static void AddEntityStateTypes(EntityStateTypeCollection collection, ContentPack contentPack)
        {
            foreach(var type in collection.stateTypes)
            {
                if(type.stateType != null)
                {
                    contentPack.entityStateTypes.AddSingle(type.stateType);
                }
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
