using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonstorm.Components.Addressables
{
    [Obsolete]
    public class AddressableInjector : MonoBehaviour
    {
        public string address;
        public Object Asset { get => _asset; private set => _asset = value; }
        [NonSerialized] private Object _asset;

        [SerializeField] private Component targetComponent;
        [SerializeField] private string targetMemberInfoName;

        private MemberInfo cachedMemberInfo;

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            throw new System.NotImplementedException();
        }

        private void Inject(MemberInfo memberInfo)
        {
            throw new System.NotImplementedException();
        }

        private MemberInfo GetMemberInfo()
        {
            throw new System.NotImplementedException();
        }

#if UNITY_EDITOR
        private void OnDisable() => throw new System.NotImplementedException();
        private void RemoveReferencesEditor()
        {
            throw new System.NotImplementedException();
        }
#endif
    }
}