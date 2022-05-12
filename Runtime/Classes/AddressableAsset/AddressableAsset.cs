using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace Moonstorm.AddressableAssets
{
    public abstract class AddressableAsset<T> : AddressableAsset where T : UObject
    {
        public string address;
        [SerializeField]
        private T asset;
        public T Asset
        {
            get
            {
                if(asset == null)
                {
                    MSULog.Warning($"Assembly {Assembly.GetCallingAssembly()} is trying to access an {GetType()} before the address has been loaded automatically!");
                    Load();
                }
                return asset;
            }
        }

        internal sealed override void Load()
        {
            if (asset != null)
                return;
            if (string.IsNullOrEmpty(address))
                return;
            else
                LoadAsset();
        }

        protected virtual void LoadAsset()
        {
            LoadFromAddress();
        }

        public async void LoadFromAddress()
        {
            var asyncOp = Addressables.LoadAssetAsync<T>(address);
            var task = asyncOp.Task;
            asset = await task;
        }
        protected void SetAsset(T asset) => this.asset = asset;
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

        private static void FinishAdressableAssets()
        {
            foreach(AddressableAsset instance in instances)
            {
                try
                {
                    instance.Load();
                }
                catch (Exception e)
                {
                    MSULog.Error(e);
                }
            }
            OnAddressableAssetsLoaded?.Invoke();
        }

        internal abstract void Load();
    }
}