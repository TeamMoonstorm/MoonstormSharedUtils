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
    public abstract class AddressableAsset<T> : AddressableAsset where T : UObject
    {
        public string address = string.Empty;
        [SerializeField]
        private T asset = null;
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

        protected virtual async Task LoadAsset()
        {
            await LoadFromAddress();
        }

        public async Task LoadFromAddress()
        {
            var asyncOp = Addressables.LoadAssetAsync<T>(address);
            var task = asyncOp.Task;
            asset = await task;
        }
        protected async Task SetAsset(T asset)
        {
            this.asset = asset;
            await Task.CompletedTask;
        }
    }

    public abstract class AddressableAsset
    {
        internal static List<AddressableAsset> instances = new List<AddressableAsset>();

        public static Action OnAddressableAssetsLoaded;
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