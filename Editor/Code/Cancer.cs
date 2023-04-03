using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Moonstorm.EditorUtils
{
    public class Cancer : IResourceLocator
    {
        public string LocatorId => throw new NotImplementedException();

        public IEnumerable<object> Keys => throw new NotImplementedException();

        public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
        {
            throw new NotImplementedException();
        }
    }
}