using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UObject = UnityEngine.Object;

namespace Moonstorm.AddressableAssets
{
    [Obsolete("Use R2API's AddressReferencedAsset<T> Instead")]
    public abstract class AddressableAsset<T> : AddressableAsset where T : UObject
    {
        public string address = string.Empty;

        public bool UseDirectReference => useDirectReference;

        [HideInInspector]
        [SerializeField]
        protected bool useDirectReference = true;

        [SerializeField]
        protected T asset = null;

        public T Asset
        {
            get
            {
                if (asset == null && !Initialized)
                {
                    var stackTrace = new StackTrace();
                    var method = stackTrace.GetFrame(1).GetMethod();
                    MSULog.Warning($"Assembly {Assembly.GetCallingAssembly()} is trying to access an {GetType()} before AddressableAssets have initialize!" +
                        $"\n Consider using AddressableAsset.OnAddressableAssetsLoaded for running code that depends on AddressableAssets! (Method: {method.DeclaringType.FullName}.{method.Name}()");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Load();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

        public static implicit operator bool(AddressableAsset<T> addressableAsset)
        {
            return addressableAsset?.Asset;
        }

        public static implicit operator T(AddressableAsset<T> addressableAsset)
        {
            return addressableAsset?.Asset;
        }

        public static implicit operator AddressReferencedAsset<T>(AddressableAsset<T> addressableAsset)
        {
            if (addressableAsset.asset)
                return new AddressReferencedAsset<T>(addressableAsset.asset);
            else
                return new AddressReferencedAsset<T>(addressableAsset.address);
        }

    }

    [Obsolete("Replaced by R2API's new AddressReferencedAsset system.")]
    public abstract class AddressableAsset
    {
        internal static List<AddressableAsset> instances = new List<AddressableAsset>();

        public static bool Initialized { get; private set; }
        public static Action OnAddressableAssetsLoaded;
        public AddressableAsset()
        {
            instances.Add(this);
        }

        [SystemInitializer]
#pragma warning disable IDE0051 // Remove unused private members
        private static void Init()
#pragma warning restore IDE0051 // Remove unused private members
        {
            RoR2Application.onLoad += FinishAdressableAssets;
        }

        private static async void FinishAdressableAssets()
        {
            foreach (AddressableAsset instance in instances)
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
            Initialized = true;
            OnAddressableAssetsLoaded?.Invoke();
        }

        internal abstract Task Load();
    }
}