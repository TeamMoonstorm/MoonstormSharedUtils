using BepInEx;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace MSU
{
    public static class GenericUnityObjectCatalog
    {
        public static int registeredObjectsCount => _registeredObjects.Length;

        private static UObject[] _registeredObjects = Array.Empty<UObject>();
        private static Dictionary<string, GenericObjectIndex> _nameToGenericObjectIndex = new Dictionary<string, GenericObjectIndex>();

        public static event CollectGenericObjectContentProvidersDelegate collectGenericObjectContentProviders;

        public static ResourceAvailability catalogAvailability = default;

        private static bool _initialized;

        public static GenericObjectIndex FindGenericObjectIndex(string name)
        {
            ThrowIfNotInitialized();

            if (_nameToGenericObjectIndex.TryGetValue(name, out var index))
                return index;

            return GenericObjectIndex.None;
        }

        public static T GetObject<T>(GenericObjectIndex index) where T : UObject
        {
            ThrowIfNotInitialized();

            var obj = HG.ArrayUtils.GetSafe(_registeredObjects, (int)index);
            if (obj)
                return (T)obj;
            return null;
        }

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

            foreach(var contentProvider in contentProviders)
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
            for(int i = 0; i < entries.Count; i++)
            {
                try
                {
                    EnsureValidity(entries[i], validObjects);
                }
                catch(Exception e)
                {
                    MSULog.Error(e);
                }
            }

            for(int i = 0; i < validObjects.Count; i++)
            {
                var obj = validObjects[i];

                _nameToGenericObjectIndex.Add(obj.name, (GenericObjectIndex)i);
            }
            return validObjects.Select(goe => goe.unityObject).ToList();
        }

        private static void EnsureValidity(GenericObjectEntry entry, List<GenericObjectEntry> validEntries)
        {
            if(entry.name.IsNullOrWhiteSpace())
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

        public delegate void AddGenericObjectContentProviderDelegate(IGenericObjectContentProvider provider);
        public delegate void CollectGenericObjectContentProvidersDelegate(AddGenericObjectContentProviderDelegate addGameplayEventContentProvider);
        public interface IGenericObjectContentProvider
        {
            IEnumerator LoadGenericObjectsAsync(List<GenericObjectEntry> dest);
        }

        public struct GenericObjectEntry
        {
            public UObject unityObject;
            public string name => _nameOverride.IsNullOrWhiteSpace() ? unityObject.name : _nameOverride;
            private string _nameOverride;
        
            public GenericObjectEntry(UObject unityObject, string nameOverride = null)
            {
                _nameOverride = nameOverride;
                this.unityObject = unityObject;
            }
        }
    }

    public enum GenericObjectIndex
    {
        None = -1,
    }
}