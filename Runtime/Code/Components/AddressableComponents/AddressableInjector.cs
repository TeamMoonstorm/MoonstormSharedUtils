using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Moonstorm.Components.Addressables
{
    [ExecuteAlways]
    public class AddressableInjector : MonoBehaviour
    {
        public string address;
        private Object _asset;

        [SerializeField] private Component targetComponent;
        [SerializeField] private string targetMemberInfoName;

        private MemberInfo cachedMemberInfo;

        private void Awake() => Refresh();
        private void OnEnable() => Refresh();
        private void OnDisable() => RemoveReferencesEditor();

        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"Invalid address in {this}, address is null, empty, or white space");
                return;
            }

            if(!targetComponent)
            {
                Debug.LogWarning("No Target Component set");
                return;
            }

            if(string.IsNullOrEmpty(targetMemberInfoName) || string.IsNullOrWhiteSpace(targetMemberInfoName))
            {
                Debug.LogWarning($"{this}'s targetMemberInfoName is null, empty or white space");
                return;
            }

            var memberInfo = GetMemberInfo();
            if(memberInfo == null)
            {
                Debug.LogWarning($"{this} failed finding the MemberInfo to target based on the name \"{targetMemberInfoName}\". Target Component: {targetComponent}");
                return;
            }

            _asset = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Object>(address).WaitForCompletion();
            if (!_asset)
                return;

            switch(memberInfo)
            {
                case PropertyInfo pInfo:
                    pInfo.SetValue(targetComponent, _asset);
                    break;
                case FieldInfo fInfo:
                    fInfo.SetValue(targetComponent, _asset);
                    break;
            }
        }

        private MemberInfo GetMemberInfo()
        {
            if (cachedMemberInfo == null)
            {
                cachedMemberInfo = targetComponent.GetType()
                    .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(m =>
                    {
                        string memberTypeName = m.GetType().Name;
                        return memberTypeName == "MonoProperty" || memberTypeName == "MonoField" || memberTypeName == "FieldInfo" || memberTypeName == "PropertyInfo";
                    })
                    .FirstOrDefault(m => $"({m.DeclaringType.Name}) {m.Name}" == targetMemberInfoName);
            }

            return cachedMemberInfo;
        }

        private void RemoveReferencesEditor()
        {
            var memberInfo = GetMemberInfo();

            switch (memberInfo)
            {
                case PropertyInfo pInfo:
                    pInfo.SetValue(targetComponent, null);
                    break;
                case FieldInfo fInfo:
                    fInfo.SetValue(targetComponent, null);
                    break;
            }
        }
    }
}