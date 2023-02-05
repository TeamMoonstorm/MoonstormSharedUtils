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
    /// <summary>
    /// An <see cref="AddressableAsset"/> is a type of class thats used for referencing an Asset ingame.
    /// <para>The asset referenced can be either a Direct reference or a reference via an Address</para>
    /// <para>An <see cref="AddressableAsset"/> has implicit operators for casting to it's <typeparamref name="T"/> Type, and for casting into Boolean (using unity's boolean cast operator)</para>
    /// </summary>
    /// <typeparam name="T">The type of asset that's being used</typeparam>
    public abstract class AddressableAsset<T> : AddressableAsset where T : UObject
    {
        /// <summary>
        /// The Address thats used during the process of loading the asset specified in T
        /// </summary>
        public string address = string.Empty;

        /// <summary>
        /// Wether this AddressableAsset is using a Direct Reference of an asset or not
        /// </summary>
        [HideInInspector]
        [SerializeField]
        protected bool useDirectReference = true;

        /// <summary>
        /// The asset, you should really load the asset using <see cref="Asset"/> at runtime.
        /// </summary>
        [SerializeField]
        protected T asset = null;

        /// <summary>
        /// The Asset that's tied to this AddressableAsset.
        /// <para>You really shouldn't use this property before <see cref="AddressableAsset.OnAddressableAssetsLoaded"/> gets invoked.</para>
        /// </summary>
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
        /// Implicit casting operator for casting an AddressableAsset to a boolean value
        /// </summary>
        public static implicit operator bool(AddressableAsset<T> addressableAsset)
        {
            return addressableAsset?.Asset;
        }

        /// <summary>
        /// Implicit casting operator for casting an AddressableAsset to it's own managed asset
        /// </summary>
        public static implicit operator T(AddressableAsset<T> addressableAsset)
        {
            return addressableAsset?.Asset;
        }
    }

    /// <summary>
    /// <inheritdoc cref="AddressableAsset{T}"/>
    /// <para>You probably want to use <see cref="AddressableAsset{T}"/> instead</para>
    /// </summary>
    public abstract class AddressableAsset
    {
        internal static List<AddressableAsset> instances = new List<AddressableAsset>();

        public static bool Initialized { get; private set; }
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