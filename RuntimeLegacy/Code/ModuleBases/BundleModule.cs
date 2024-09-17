using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Moonstorm
{
    public abstract class BundleModule : ModuleBase<ContentBase>
    {
        public abstract AssetBundle MainBundle { get; }

        protected sealed override void InitializeContent(ContentBase contentClass)
        {
            throw new System.NotSupportedException($"A BundleModule does not have a ContentBase by definition.");
        }

        protected new IEnumerable<T> GetContentClasses<T>(Type excludedType = null) where T : ContentBase
        {
            throw new System.NotSupportedException($"A BundleModule does not have a ContentBase by definition.");
        }

        public TObject Load<TObject>(string name) where TObject : UObject
        {
            return MainBundle.LoadAsset<TObject>(name);
        }

        public TObject[] LoadAll<TObject>() where TObject : UObject
        {
            return MainBundle.LoadAllAssets<TObject>();
        }
    }
}
