using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.AddressableAssets;

namespace Moonstorm.AddressableAssets
{
    public abstract class AddressableAsset<T> : AddressableAsset where T : UObject
    {
        [SerializeField]
        internal string address;
        [SerializeField]
        private T asset;
        public T Asset => asset;

        internal sealed override void Load()
        {
            if (asset != null)
                return;
            if (string.IsNullOrEmpty(address))
                return;
            else
                LoadAsset();
        }

        public virtual void LoadAsset()
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
        }

        internal abstract void Load();
    }
}