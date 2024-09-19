using BepInEx;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UObject = UnityEngine.Object;

namespace MSU
{
    /// <summary>
    /// The <see cref="GenericUnityObjectCatalog"/> is a catalog used to network any kind of object that inherits from <see cref="UnityEngine.Object"/>
    /// <para>Currently it's used by the <see cref="GameplayEventTextController.EventTextRequest"/>'s systems to obtain a TMPro Font Asset for custom displaying of the events.</para>
    /// <br>To add new Unity Objects tot he catalog, implement <see cref="IGenericObjectContentProvider"/> and utilize <see cref="collectGenericObjectContentProviders"/>'s event to add your provider.</br>
    /// </summary>
    public static class GenericUnityObjectCatalog
    {
        /// <summary>
        /// The total amount of registered objects
        /// </summary>
        public static int registeredObjectsCount => _registeredObjects.Length;

        private static UObject[] _registeredObjects = Array.Empty<UObject>();
        private static Dictionary<string, GenericObjectIndex> _nameToGenericObjectIndex = new Dictionary<string, GenericObjectIndex>();

        /// <summary>
        /// Event ran when the <see cref="GenericUnityObjectCatalog"/> is collectiong all the content providers, from which the UnityEngine.Objects will be obtained
        /// </summary>
        public static event CollectGenericObjectContentProvidersDelegate collectGenericObjectContentProviders;

        /// <summary>
        /// Represents the availability of this catalog.
        /// </summary>
        public static ResourceAvailability catalogAvailability = default;

        private static bool _initialized;

        /// <summary>
        /// Finds a <see cref="GenericObjectIndex"/> with the name <paramref name="name"/> and returns it.
        /// <br>Throws an exception if the catalog has not been initialized</br>
        /// </summary>
        /// <param name="name">The name of the object to find</param>
        /// <returns>A valid <see cref="GenericObjectIndex"/>, or <see cref="GenericObjectIndex.None"/> if no Generic Object is found.</returns>
        public static GenericObjectIndex FindGenericObjectIndex(string name)
        {
            ThrowIfNotInitialized();

            if (_nameToGenericObjectIndex.TryGetValue(name, out var index))
                return index;

            return GenericObjectIndex.None;
        }

        /// <summary>
        /// Retrieves the Object associated to the specified <see cref="GenericObjectIndex"/> specified by <paramref name="index"/>, casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="index">The index of the object</param>
        /// <returns>The object, null if the index is invalid</returns>
        public static T GetObject<T>(GenericObjectIndex index) where T : UObject
        {
            ThrowIfNotInitialized();

            var obj = HG.ArrayUtils.GetSafe(_registeredObjects, (int)index);
            if (obj)
                return (T)obj;
            return null;
        }

        /// <summary>
        /// Retrieves the Object associated to the specifeid <see cref="GenericObjectIndex"/> specified by <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the object</param>
        /// <returns>The object, null if the index is invalid.</returns>
        public static UObject GetObject(GenericObjectIndex index)
        {
            ThrowIfNotInitialized();

            return HG.ArrayUtils.GetSafe(_registeredObjects, (int)index);
        }

        [SystemInitializer]
        private static IEnumerator Init()
        {
            MSULog.Info("Initializing the GenericObject Catalog...");
            List<IGenericObjectContentProvider> contentProviders = new List<IGenericObjectContentProvider>();

            collectGenericObjectContentProviders?.Invoke(AddGenericObjectContentProvider);

            List<GenericObjectEntry> loadedObjects = new List<GenericObjectEntry>();

            ParallelMultiStartCoroutine coroutine = new ParallelMultiStartCoroutine();

            foreach (var contentProvider in contentProviders)
            {
                yield return null;
                coroutine.Add(contentProvider.LoadGenericObjectsAsync, loadedObjects);
            }

            coroutine.Start();
            while (!coroutine.isDone)
                yield return null;

            _nameToGenericObjectIndex.Clear();

            loadedObjects = loadedObjects.OrderBy(obj => obj.name).ToList();

            _registeredObjects = RegisterObjects(loadedObjects).ToArray();

            _initialized = true;
            catalogAvailability.MakeAvailable();

            void AddGenericObjectContentProvider(IGenericObjectContentProvider contentProvider)
            {
                contentProviders.Add(contentProvider);
            }
        }

        private static List<UObject> RegisterObjects(List<GenericObjectEntry> entries)
        {
            List<GenericObjectEntry> validObjects = new List<GenericObjectEntry>();
            for (int i = 0; i < entries.Count; i++)
            {
                try
                {
                    EnsureValidity(entries[i], validObjects);
                }
                catch (Exception e)
                {
                    MSULog.Error(e);
                }
            }

            for (int i = 0; i < validObjects.Count; i++)
            {
                var obj = validObjects[i];

                _nameToGenericObjectIndex.Add(obj.name, (GenericObjectIndex)i);
            }
            return validObjects.Select(goe => goe.unityObject).ToList();
        }

        private static void EnsureValidity(GenericObjectEntry entry, List<GenericObjectEntry> validEntries)
        {
            if (entry.name.IsNullOrWhiteSpace())
            {
                throw new NullReferenceException("Entry's name is null");
            }

            validEntries.Add(entry);
        }

        private static void ThrowIfNotInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("GenericObjectCatalog not Initialized. Use \"catalogAvailability\" to call a method when the GenericObjectCatalog is initialized.");
            }
        }

        /// <summary>
        /// Delegate used to add a new <see cref="IGenericObjectContentProvider"/> to the catalog
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public delegate void AddGenericObjectContentProviderDelegate(IGenericObjectContentProvider provider);

        /// <summary>
        /// Delegate used to collect <see cref="IGenericObjectContentProvider"/> to the catalog.
        /// </summary>
        /// <param name="addGameplayEventContentProvider">The Add method</param>
        public delegate void CollectGenericObjectContentProvidersDelegate(AddGenericObjectContentProviderDelegate addGameplayEventContentProvider);

        /// <summary>
        /// An interface you can implement to add new entries to the <see cref="GenericUnityObjectCatalog"/>
        /// </summary>
        public interface IGenericObjectContentProvider
        {
            /// <summary>
            /// Implement this interface to load any objects you'd like to add to the <see cref="GenericUnityObjectCatalog"/>
            /// </summary>
            /// <param name="dest">The list with all the Generic Objects, do not remove, clear or replace this list, only add to it.</param>
            /// <returns>a Coroutine that the catalog uses to await the process.</returns>
            IEnumerator LoadGenericObjectsAsync(List<GenericObjectEntry> dest);
        }

        /// <summary>
        /// Represents a generic object entry during the catalog initialization step
        /// </summary>
        public struct GenericObjectEntry
        {
            /// <summary>
            /// The Unity Object we're registring
            /// </summary>
            public UObject unityObject;

            /// <summary>
            /// The name used within the catalog
            /// </summary>
            public string name => _nameOverride.IsNullOrWhiteSpace() ? unityObject.name : _nameOverride;
            private string _nameOverride;

            /// <summary>
            /// Constructor for <see cref="GenericObjectEntry"/>
            /// </summary>
            /// <param name="unityObject">The object to add to the catalog</param>
            /// <param name="nameOverride">If supplied, this name will be associated to the index instead of <paramref name="unityObject"/>'s name</param>
            public GenericObjectEntry(UObject unityObject, string nameOverride = null)
            {
                _nameOverride = nameOverride;
                this.unityObject = unityObject;
            }
        }
    }

    /// <summary>
    /// Represents an Index for a unity object in the <see cref="GenericUnityObjectCatalog"/>, these are assigned to different objects at runtime
    /// </summary>
    public enum GenericObjectIndex
    {
        /// <summary>
        /// Represents an invalid object
        /// </summary>
        None = -1,
    }
}