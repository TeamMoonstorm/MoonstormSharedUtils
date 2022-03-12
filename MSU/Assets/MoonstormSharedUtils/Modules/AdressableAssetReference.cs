using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm
{
    [Serializable]
    public struct AdressableAssetReference<TAsset> where TAsset : UnityEngine.Object
    {
        public string adress;
        public TAsset Asset
        {
            get
            {
                if (_asset)
                    return _asset;

                try
                {
                    //Try getting asset from adress
                    var asset = Addressables.LoadAssetAsync<TAsset>(adress).WaitForCompletion();
                    if(asset)
                    {
                        asset = _asset;
                        return _asset;
                    }
                    else
                    {
                        throw new NullReferenceException($"Could not find asset with adress {adress}");
                    }
                }
                catch(Exception e) 
                {
                    MSULog.Error(e);
                    return null;
                }
            }
        }
        [SerializeField]
        private TAsset _asset;
    }
}
