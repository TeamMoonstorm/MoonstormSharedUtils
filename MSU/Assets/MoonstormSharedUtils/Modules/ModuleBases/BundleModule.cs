using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm
{
    public abstract class BundleModule : ModuleBase<ContentBase>
    {
        public abstract AssetBundle MainBundle { get; }

        protected override bool InitializeContent(ContentBase contentClass)
        {
            throw new System.NotSupportedException($"A BundleModule does not have a ContentBase by definition.");
        }

        public TObject Load<TObject>(string name) where TObject : Object
        {
            return MainBundle.LoadAsset<TObject>(name);
        }

        public TObject[] LoadAll<TObject>() where TObject : Object
        {
            return MainBundle.LoadAllAssets<TObject>();
        }
    }
}
