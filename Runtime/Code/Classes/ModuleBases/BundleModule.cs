using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Moonstorm
{
    /// <summary>
    /// A class that extends ModuleBase
    /// <para>Unlike regular module bases, a BundleModule [Does Not] manage a ContentBase class.</para>
    /// <para>The bundle module is used for managing miscelaneous ordeals with AssetBundles</para>
    /// </summary>
    public abstract class BundleModule : ModuleBase<ContentBase>
    {
        /// <summary>
        /// Your mod's main asset bundle
        /// </summary>
        public abstract AssetBundle MainBundle { get; }

        /// <summary>
        /// Do not call this method, as stated in the <see cref="BundleModule"/> documentation, BundleModules do not have content classes
        /// <para>This throws a <see cref="NotSupportedException"/></para>
        /// </summary>
        protected sealed override void InitializeContent(ContentBase contentClass)
        {
            throw new System.NotSupportedException($"A BundleModule does not have a ContentBase by definition.");
        }

        /// <summary>
        /// Do not call this method, as stated in the <see cref="BundleModule"/> documentation, BundleModules do not have content classes
        /// <para>This throws a <see cref="NotSupportedException"/></para>
        /// </summary>
        protected IEnumerable<T> GetContentClasses<T>(Type excludedType = null) where T : ContentBase
        {
            throw new System.NotSupportedException($"A BundleModule does not have a ContentBase by definition.");
        }

        /// <summary>
        /// Loads an asset of type <typeparamref name="TObject"/> from <see cref="MainBundle"/>
        /// </summary>
        /// <typeparam name="TObject">The type of object to load</typeparam>
        /// <param name="name">The name of the object to load</param>
        /// <returns>The loaded object</returns>
        public TObject Load<TObject>(string name) where TObject : UObject
        {
            return MainBundle.LoadAsset<TObject>(name);
        }

        /// <summary>
        /// Loads all assets of type <typeparamref name="TObject"/> from <see cref="MainBundle"/>
        /// </summary>
        /// <typeparam name="TObject">The type of objects to load</typeparam>
        /// <returns>An array of all objects of type <typeparamref name="TObject"/></returns>
        public TObject[] LoadAll<TObject>() where TObject : UObject
        {
            return MainBundle.LoadAllAssets<TObject>();
        }
    }
}
