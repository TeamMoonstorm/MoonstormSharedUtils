using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MSU
{
    /// <summary>
    /// Class containing a plethora of utility methods regarding Content and Content related classes from Risk of Rain 2
    /// </summary>
    public static class ContentUtil
    {
        //Dummy expansion def, we dont add it to the Catalog to make it so stuff thats disabled cant be added.
        private static ExpansionDef _dummyExpansion = ScriptableObject.CreateInstance<ExpansionDef>();

        /// <summary>
        /// Disables the provided ArtifactDef by setting it's <see cref="ArtifactDef.requiredExpansion"/> to a Dummy ExpansionDef thats not added to the ExpansionCatalog.
        /// </summary>
        /// <param name="artifactDef">The ArtifactDef to Disable.</param>
        public static void DisableArtifact(ArtifactDef artifactDef)
        {
            //setting expansion to an invalid value makes it impossible for it to appear in lobby.
            artifactDef.requiredExpansion = _dummyExpansion;
        }

        /// <summary>
        /// Disables the provided Survivor by setting it's <see cref="SurvivorDef.hidden"/> boolean to True.
        /// </summary>
        /// <param name="survivorDef">The Survivor to Disable</param>
        public static void DisableSurvivor(SurvivorDef survivorDef)
        {
            survivorDef.hidden = true;
        }

        /// <summary>
        /// Disables the provided ItemDef by setting it's required <see cref="ItemDef.requiredExpansion"/> to a Dummy ExpansionDef thats not added to the ExpansionCatalog.
        /// </summary>
        /// <param name="itemDef">The Item to Disable.</param>
        public static void DisableItem(ItemDef itemDef)
        {
            itemDef.requiredExpansion = _dummyExpansion;
        }

        /// <summary>
        /// Disables the provided EquipmentDef by setting it's required <see cref="EquipmentDef.requiredExpansion"/> to a Dummy ExpansionDef thats not added to the ExpansionCatalog
        /// </summary>
        /// <param name="equipmentDef">The Equipment to Disable</param>
        public static void DisableEquipment(EquipmentDef equipmentDef)
        {
            equipmentDef.requiredExpansion = _dummyExpansion;
        }

        /// <summary>
        /// See also <see cref="IContentPieceProvider"/>
        /// <para>Creates a new, non generic ContentPieceProvider for the specified interface that inherits from <see cref="IContentPiece"/>, which is the typeParam <typeparamref name="TContentPieceType"/>. This is done by analyzing the assembly from <paramref name="plugin"/> and creating new instances of classes that implement <typeparamref name="TContentPieceType"/></para>
        /// <br>This is particularly useful for creating a ContentPieceProvider for the <see cref="VanillaSurvivorModule"/>, where you can just set <typeparamref name="TContentPieceType"/> to <see cref="IVanillaSurvivorContentPiece"/></br>
        /// </summary>
        /// <typeparam name="TContentPieceType">The type of interface the provider will provide</typeparam>
        /// <param name="plugin">The plugin to scan for content pieces</param>
        /// <param name="contentPack">The plugin's content pack</param>
        /// <returns>An IContentPieceProvider with <paramref name="plugin"/>'s classes that implement <typeparamref name="TContentPieceType"/></returns>
        public static IContentPieceProvider CreateContentPieceProvider<TContentPieceType>(BaseUnityPlugin plugin, ContentPack contentPack) where TContentPieceType : IContentPiece
        {
            return new ContentPieceProvider(AnalyzeForContentPieces<TContentPieceType>(plugin).OfType<IContentPiece>(), contentPack);
        }

        /// <summary>
        /// Analyzes the <paramref name="plugin"/>'s Assembly for classes that inherit from <see cref="IContentPiece"/>, which is the typeParam <typeparamref name="TContentPieceType"/>.
        /// <br>This is ideal for creating your own implementation of <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <typeparam name="TContentPieceType">The type of interface the provider will provide</typeparam>
        /// <param name="plugin">The plugin to scan for content pieces</param>
        /// <returns>An Enumerable of <typeparamref name="TContentPieceType"/></returns>
        public static IEnumerable<TContentPieceType> AnalyzeForContentPieces<TContentPieceType>(BaseUnityPlugin plugin) where TContentPieceType : IContentPiece
        {
            var assembly = plugin.GetType().Assembly;
            return ReflectionCache.GetTypes(assembly)
                .Where(PassesFilterForContentPieceInterface<TContentPieceType>)
                .Select(t => (TContentPieceType)Activator.CreateInstance(t));
        }

        /// <summary>
        /// See also <see cref="IContentPieceProvider"/>
        /// <para>Creates a new, generic ContentPieceProvider for the specified unity Object, this is done by analyzing the assembly from <paramref name="baseUnityPlugin"/> and creating new instances of classes that implement <see cref="IContentPiece{T}"/>.</para>
        /// </summary>
        /// <typeparam name="TUObjectType">The type of object that the provider provides.</typeparam>
        /// <param name="baseUnityPlugin">The plugin to scan for content pieces</param>
        /// <param name="contentPack">The plugin's ContentPack</param>
        /// <returns>An IContentPieceProvider with <paramref name="baseUnityPlugin"/>'s classes that implement <see cref="IContentPiece{T}"/></returns>
        public static IContentPieceProvider<TUObjectType> CreateGenericContentPieceProvider<TUObjectType>(BaseUnityPlugin baseUnityPlugin, ContentPack contentPack) where TUObjectType : UnityEngine.Object
        {
            return new GenericContentPieceProvider<TUObjectType>(AnalyzeForGenericContentPieces<TUObjectType>(baseUnityPlugin), contentPack);
        }

        /// <summary>
        /// Analyzes the <paramref name="baseUnityPlugin"/>'s Assembly for classes that implement <see cref="IContentPiece{T}"/> and returns a collection of said classes's instances.
        /// <br>This is ideal for creating your own implementation of <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <typeparam name="TUObjectType">The type of object to analyze for.</typeparam>
        /// <param name="baseUnityPlugin">The plugin to analyze for content pieces</param>
        /// <returns>An Enumerable of <see cref="IContentPiece{T}"/></returns>
        public static IEnumerable<IContentPiece<TUObjectType>> AnalyzeForGenericContentPieces<TUObjectType>(BaseUnityPlugin baseUnityPlugin) where TUObjectType : UnityEngine.Object
        {
            var assembly = baseUnityPlugin.GetType().Assembly;
            return ReflectionCache.GetTypes(assembly)
                .Where(PassesFilterForObjectType<TUObjectType>)
                .Select(t => (IContentPiece<TUObjectType>)Activator.CreateInstance(t));
        }

        /// <summary>
        /// See also <see cref="IContentPieceProvider"/>
        /// <para>Creates a new, generic ContentPieceProvider that provides GameObjects with a specific component specified by <typeparamref name="TComponentType"/>, this is done by analyzing the assembly from <paramref name="baseUnityPlugin"/> and creating new instances of classes that implement <see cref="IGameObjectContentPiece{T}"/>.</para>
        /// </summary>
        /// <typeparam name="TComponentType">The component that the game objects have that the provider provides.</typeparam>
        /// <param name="baseUnityPlugin">The plugin to scan for content pieces</param>
        /// <param name="contentPack">The plugin's ContentPack</param>
        /// <returns>An IContentPieceProvider with <paramref name="baseUnityPlugin"/>'s classes that implement <see cref="IGameObjectContentPiece{T}"/></returns>
        public static IContentPieceProvider<GameObject> CreateGameObjectGenericContentPieceProvider<TComponentType>(BaseUnityPlugin baseUnityPlugin, ContentPack contentPack)
        {
            return new GenericContentPieceProvider<GameObject>(AnalyzeForGameObjectGenericContentPieces<TComponentType>(baseUnityPlugin), contentPack);
        }

        /// <summary>
        /// Analyzes the <paramref name="baseUnityPlugin"/>'s Assembly for classes that implement <see cref="IGameObjectContentPiece{T}"/> and returns a collection of said classes's instances.
        /// <br>This is ideal for creating your own implementation of <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <typeparam name="TComponentTYpe">The type of component to analyze for.</typeparam>
        /// <param name="baseUnityPlugin">The plugin to analyze for content pieces</param>
        /// <returns>An Enumerable of IContentPiece that have gameObjects with the component specified in <typeparamref name="TComponentTYpe"/>></returns>
        public static IEnumerable<IContentPiece<GameObject>> AnalyzeForGameObjectGenericContentPieces<TComponentTYpe>(BaseUnityPlugin baseUnityPlugin)
        {
            var assembly = baseUnityPlugin.GetType().Assembly;

            return ReflectionCache.GetTypes(assembly)
                .Where(t => PassesFilterForObjectType<GameObject>(t) && t.GetInterfaces().Contains(typeof(IGameObjectContentPiece<TComponentTYpe>)))
                .Select(t => (IContentPiece<GameObject>)Activator.CreateInstance(t));
        }

        /// <summary>
        /// Adds a single ContentPiece of type <typeparamref name="T"/> to a NamedAssetCollection of type <typeparamref name="T"/>
        /// <br>Utilize this instead of <see cref="NamedAssetCollection{T}.Add(T[])"/> if you only want to add one asset.</br>
        /// </summary>
        /// <typeparam name="T">The type of the Asset</typeparam>
        /// <param name="collection">The collection that will be modified</param>
        /// <param name="content">The content to add</param>
        public static void AddSingle<T>(this NamedAssetCollection<T> collection, T content) where T : class
        {
            string name = collection.nameProvider(content);
            string backupName = $"{content.GetType().Name}_{collection.Count}";

            bool assetInCollection = collection.assetToName.ContainsKey(content);
            if (assetInCollection)
            {
                return;
            }

            if (name.IsNullOrWhiteSpace())
            {
#if DEBUG
                MSULog.Warning($"Content {content} does not have a valid name! ({name}). assigning a generic name...");
#endif
                name = backupName;
            }

            if (collection.nameToAsset.ContainsKey(name))
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

        /// <summary>
        /// Populates public static fields of type <typeparamref name="TAsset"/> that are found in <paramref name="typeToPopulate"/> utilizing the assets found in <paramref name="assets"/>.
        /// <br>Unlike the base game's <see cref="ContentLoadHelper.PopulateTypeFields{TAsset}(Type, NamedAssetCollection{TAsset}, Func{string, string})"/>. This version logs assets that did not have a corresponding field found in <paramref name="typeToPopulate"/></br>
        /// </summary>
        /// <typeparam name="TAsset">The type of asset to populate</typeparam>
        /// <param name="typeToPopulate">The actual type to populate.</param>
        /// <param name="assets">The AssetCollection to use for population.</param>
        public static void PopulateTypeFields<TAsset>(Type typeToPopulate, NamedAssetCollection<TAsset> assets) where TAsset : UnityEngine.Object
        {
#if DEBUG
            MSULog.Info($"Attempting to populate {typeToPopulate.FullName} with {assets.Count} assets");
#endif

            string[] array = new string[assets.Length];

#if DEBUG
            List<TAsset> notAssignedAssets = assets.assetInfos.Select(item => item.asset).ToList();
            StringBuilder failureLog = new StringBuilder();
#endif

            for (int i = 0; i < assets.Length; i++)
            {
                array[i] = assets[i].name;
            }

            int missingAssets = 0;
            FieldInfo[] fields = typeToPopulate.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.FieldType.IsSameOrSubclassOf(typeof(TAsset)))
                {
                    TargetAssetNameAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<TargetAssetNameAttribute>(fieldInfo);
                    string name = ((customAttribute != null) ? customAttribute.targetAssetName : fieldInfo.Name);
                    TAsset val = assets.Find(name);
                    if (val != null)
                    {
#if DEBUG
                        notAssignedAssets.Remove(val);
#endif
                        fieldInfo.SetValue(null, val);

                        continue;
                    }

                    missingAssets++;
#if DEBUG
                    failureLog.AppendLine($"Failed to assign {fieldInfo.DeclaringType.FullName}.{fieldInfo.Name}: Asset Not Found.");
#endif
                }
            }

#if DEBUG
            if (failureLog.Length > 1)
            {
                failureLog.Insert(0, $"Failed to assign {missingAssets} field(s), logging which ones have failed.");
                MSULog.Warning(failureLog.ToString());
                failureLog.Clear();
            }

            if (notAssignedAssets.Count > 0)
            {
                failureLog.AppendLine($"There where {notAssignedAssets} Assets that have not been assigned to fields inside {typeToPopulate.FullName}. Listing assets:");
                foreach (var asset in notAssignedAssets)
                {
                    failureLog.AppendLine(asset.name);
                }
                MSULog.Warning(failureLog);
            }
#endif
        }

        /// <summary>
        /// Adds all and any Content pieces from the AssetCollection found in <paramref name="assetCollection"/> to the ContentPack specified in <paramref name="contentPack"/>.
        /// <br>This is ideal to store a Content's required assets. For example, you can have a <see cref="ISurvivorContentPiece"/> that calls this method inside <see cref="IContentPackModifier.ModifyContentPack(ContentPack)"/> to add the survivor's states, skillDefs, that are found inside the survivor's AssetCollection.</br>
        /// </summary>
        /// <param name="contentPack">The content pack to modify</param>
        /// <param name="assetCollection">The asset collection to use.</param>
        public static void AddContentFromAssetCollection(this ContentPack contentPack, AssetCollection assetCollection)
        {
            AddContentFromCollectionInternal(contentPack, assetCollection.assets);
        }

        /// <summary>
        /// <inheritdoc cref="AddContentFromAssetCollection(ContentPack, AssetCollection)"/>
        /// <para>Unlike the 2 argument overload, this version of the method accepts a predicate to filter assets from being added to the collection. This can be done for example to avoid adding the UnlockableDefs from an asset.</para>
        /// </summary>
        /// <param name="contentPack">The content pack to modify</param>
        /// <param name="assetCollection">The asset collection to use.</param>
        /// <param name="predicate">A predicate to filter assets, cannot be null.</param>
        public static void AddContentFromAssetCollection(this ContentPack contentPack, AssetCollection assetCollection, Func<UnityEngine.Object, bool> predicate)
        {
            var filtered = assetCollection.assets.Where(predicate).ToArray();
            AddContentFromCollectionInternal(contentPack, filtered);
        }

        /// <summary>
        /// Finds an asset of name <paramref name="assetName"/> and type <typeparamref name="TAsset"/> inside an AssetCollection
        /// </summary>
        /// <typeparam name="TAsset">The type of the asset to find</typeparam>
        /// <param name="collection">The collection that'll be searched</param>
        /// <param name="assetName">The asset's name</param>
        /// <returns>The found asset, null if no asset was found</returns>
        public static TAsset FindAsset<TAsset>(this AssetCollection collection, string assetName) where TAsset : UnityEngine.Object
        {
            var result = collection.assets.OfType<TAsset>().Where(asset => string.Equals(asset.name, assetName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
#if DEBUG
            if (!result)
            {
                MSULog.Warning($"Asset of type {typeof(TAsset).Name} and name {assetName} was not found in {collection}, are you sure youre typing the asset's name correctly?");
            }
#endif
            return result;
        }

        /// <summary>
        /// Finds all assets of type <typeparamref name="TAsset"/> inside an AssetCollection
        /// </summary>
        /// <typeparam name="TAsset">The type of the asset to find</typeparam>
        /// <param name="collection">The collection that'll be searched</param>
        /// <returns>An array of <typeparamref name="TAsset"/>, if no assets where found, it returns an empty array.</returns>
        public static TAsset[] FindAssets<TAsset>(this AssetCollection collection) where TAsset : UnityEngine.Object
        {
            var result = collection.assets.OfType<TAsset>().ToArray();

#if DEBUG
            if (result.Length == 0)
            {
                MSULog.Warning($"Could not find any assets of type {typeof(TAsset).Name} inside {collection}");
            }
#endif
            return result;
        }

        private static void AddContentFromCollectionInternal(ContentPack contentPack, UnityEngine.Object[] assetCollection)
        {
            foreach (var asset in assetCollection)
            {
                try
                {
                    if (!asset)
                        continue;

                    HandleAssetAddition(asset, contentPack);
                }
                catch (Exception e)
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
            if (go.TryGetComponent<CharacterBody>(out var bodyComponent))
            {
                isNetworkedByDefault = true;
                contentPack.bodyPrefabs.AddSingle(go);
            }
            if (go.TryGetComponent<CharacterMaster>(out var masterComponent))
            {
                isNetworkedByDefault = true;
                contentPack.masterPrefabs.AddSingle(go);
            }
            if (go.TryGetComponent<ProjectileController>(out var controllerComponent))
            {
                isNetworkedByDefault = true;
                contentPack.projectilePrefabs.AddSingle(go);
            }
            if (go.TryGetComponent<Run>(out var runComponent))
            {
                isNetworkedByDefault = true;
                contentPack.gameModePrefabs.AddSingle(go);
            }
            if (go.TryGetComponent<EffectComponent>(out var effectComponent))
            {
                contentPack.effectDefs.AddSingle(new EffectDef(go));
            }

            if (identity && !isNetworkedByDefault)
            {
                contentPack.networkedObjectPrefabs.AddSingle(go);
            }
        }

        private static void AddEntityStateTypes(EntityStateTypeCollection collection, ContentPack contentPack)
        {
            foreach (var type in collection.stateTypes)
            {
                if (type.stateType != null)
                {
                    contentPack.entityStateTypes.AddSingle(type.stateType);
                }
            }
        }

        private static bool PassesFilterForContentPieceInterface<TContentPieceInterface>(Type t) where TContentPieceInterface : IContentPiece
        {
            bool notAbstract = !t.IsAbstract;
            bool implementsInterface = t.GetInterfaces().Contains(typeof(TContentPieceInterface));
            return notAbstract && implementsInterface;
        }
        private static bool PassesFilterForObjectType<T>(Type t) where T : UnityEngine.Object
        {
            bool notAbstract = !t.IsAbstract;
            bool implementsInterface = t.GetInterfaces().Contains(typeof(IContentPiece<T>));
            return notAbstract && implementsInterface;
        }

        private class ContentPieceProvider : IContentPieceProvider
        {
            public ContentPack contentPack => _contentPack;

            private ContentPack _contentPack;
            private IContentPiece[] _contentPieces;

            public IContentPiece[] GetContents()
            {
                return _contentPieces;
            }

            public ContentPieceProvider(IEnumerable<IContentPiece> contentPieces, ContentPack contentPack)
            {
                _contentPieces = contentPieces.ToArray();
                _contentPack = contentPack;
            }
        }

        private class GenericContentPieceProvider<T> : IContentPieceProvider<T> where T : UnityEngine.Object
        {
            public ContentPack contentPack => _contentPack;


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
