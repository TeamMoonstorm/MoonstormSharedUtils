using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.AddressableAssets;
using System.Reflection;
using System.Threading.Tasks;

namespace Moonstorm.AddressableAssets
{
    /// <summary>
    /// An <see cref="AddressableAsset"/> is a type of class thats used for referencing an Asset ingame.
    /// <para>The asset referenced can be either a Direct reference or a reference via an Address</para>
    /// </summary>
    /// <typeparam name="T">The type of asset that's being used</typeparam>
    public abstract class AddressableAsset<T> : AddressableAsset where T : UObject
    {
        /// <summary>
        /// The Address thats used during the process of loading the asset specified in T
        /// </summary>
        public string address = string.Empty;
        [SerializeField]
        private T asset = null;

        /// <summary>
        /// The Asset that's tied to this AddressableAsset.
        /// <para>You really should use this property before <see cref="AddressableAsset.OnAddressableAssetsLoaded"/> gets invoked.</para>
        /// </summary>
        public T Asset
        {
            get
            {
                if(asset == null)
                {
                    MSULog.Warning($"Assembly {Assembly.GetCallingAssembly()} is trying to access an {GetType()} before AddressableAssets have initialize!" +
                        $"\n Consider using AddressableAsset.OnAddressableAssetsLoaded for running code that depends on AddressableAssets!");
                    Load();
                }
                return asset;
            }
        }

        internal sealed override async Task Load()
        {
            if (asset == null && !string.IsNullOrEmpty(address))
                await LoadAsset();
        }

        /// <summary>
        /// Implement here how the Asset gets loaded when <see cref="Asset"/> is null
        /// </summary>
        /// <returns></returns>
        protected virtual async Task LoadAsset()
        {
            await LoadFromAddress();
        }

        /// <summary>
        /// Loads the Asset via Addressables
        /// </summary>
        /// <returns>Returns a Task when the operation is complete</returns>
        public async Task LoadFromAddress()
        {
            var asyncOp = Addressables.LoadAssetAsync<T>(address);
            var task = asyncOp.Task;
            asset = await task;
        }
        /// <summary>
        /// Sets the Asset for this Addressable Asset
        /// </summary>
        /// <param name="asset">The asset</param>
        /// <returns>Task.CompletedTask</returns>
        protected async Task SetAsset(T asset)
        {
            this.asset = asset;
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// <para>You probably want to use <see cref="AddressableAsset{T}"/> instead</para>
    /// </summary>
    public abstract class AddressableAsset
    {
        internal static List<AddressableAsset> instances = new List<AddressableAsset>();

        /// <summary>
        /// An Action that gets invoked when all the AddressableAssets have been loaded
        /// </summary>
        public static Action OnAddressableAssetsLoaded;

        /// <summary>
        /// Constructor
        /// </summary>
        public AddressableAsset()
        {
            instances.Add(this);
        }

        [SystemInitializer]
        private static void Init()
        {
            RoR2Application.onLoad += FinishAdressableAssets;
        }

        private static async void FinishAdressableAssets()
        {
            foreach(AddressableAsset instance in instances)
            {
                try
                {
                    await instance.Load();
                }
                catch (Exception e)
                {
                    MSULog.Error(e);
                }
            }
            OnAddressableAssetsLoaded?.Invoke();
        }

        internal abstract Task Load();
    }
}