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
    /// <summary>
    /// Component that injects an AddressableAsset to a component's field
    /// </summary>
    [ExecuteAlways]
    public class AddressableInjector : MonoBehaviour
    {
        [Tooltip("The address used for injecting")]
        public string address;
        private Object _asset;

        [Tooltip("The component that will be injected")]
        [SerializeField] private Component targetComponent;
        [Tooltip("The member info that'll be injected")]
        [SerializeField] private string targetMemberInfoName;

        private MemberInfo cachedMemberInfo;

        private void Awake() => Refresh();
        private void OnEnable() => Refresh();

        /// <summary>
        /// Refreshes and re-injects the asset specified in <see cref="address"/>
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(address))
            {
#if DEBUG
                string msg = $"Invalid address in {this}, address is null, empty, or white space";
#if UNITY_EDITOR
                Debug.LogWarning(msg);
#else
                MSULog.Warning(msg);
#endif
#endif
                return;
            }

            if(!targetComponent)
            {
#if DEBUG
                string msg = $"No Target Component Set in {this}";
#if UNITY_EDITOR
                Debug.LogWarning(msg);
#else
                MSULog.Warninig(msg);
#endif
#endif
                return;
            }

            if(string.IsNullOrEmpty(targetMemberInfoName) || string.IsNullOrWhiteSpace(targetMemberInfoName))
            {
#if DEBUG
                string msg = $"{this}'s targetMemberInfoName is null, empty or white space";
#if UNITY_EDITOR
                Debug.LogWarning(msg);
#else
                MSULog.Warning(msg);
#endif
#endif
                return;
            }

            var memberInfo = GetMemberInfo();
            if (memberInfo == null)
            {
#if DEBUG
                string msg = $"{this} failed finding the MemberInfo to target based on the name \"{targetMemberInfoName}\". Target Component: {targetComponent}";
#if UNITY_EDITOR
                Debug.LogWarning(msg);
#else
                MSULog.Warning(msg);
#endif
#endif
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
            if (cachedMemberInfo == null && targetComponent)
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

#if UNITY_EDITOR
        private void OnDisable() => RemoveReferencesEditor();
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
#endif
    }
}