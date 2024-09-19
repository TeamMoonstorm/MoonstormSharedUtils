using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components.Addressables
{

    [ExecuteAlways]
    public class InstantiateAddressablePrefab : MonoBehaviour
    {
        [SerializeField] private string address;

        [SerializeField] private bool setPositionAndRotationToZero;

        [SerializeField] private bool useLocalPositionAndRotation;

        [SerializeField] private bool refreshInEditor;
        [SerializeField, HideInInspector] private bool hasNetworkIdentity;

        public GameObject Instance => instance;
        [NonSerialized]
        private GameObject instance;

        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            if (instance)
                MSUtil.DestroyImmediateSafe(instance, true);
        }

        public void Refresh()
        {
            if (Application.isEditor && !refreshInEditor)
                return;

            if (instance)
            {
                MSUtil.DestroyImmediateSafe(instance, true);
            }

            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(address))
            {
                MSULog.Warning($"Invalid address in {this}, address is null, empty, or white space");
                return;
            }

            GameObject prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion();
            hasNetworkIdentity = prefab.GetComponent<NetworkIdentity>();

            if (hasNetworkIdentity && !Application.isEditor)
            {
                if (NetworkServer.active)
                {
                    instance = Instantiate(prefab, transform);
                    NetworkServer.Spawn(instance);
                }
            }
            else
            {
                instance = Instantiate(prefab, transform);
            }

            instance.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
            foreach (Transform t in instance.GetComponentsInChildren<Transform>())
            {
                t.gameObject.hideFlags = instance.hideFlags;
            }
            if (setPositionAndRotationToZero)
            {
                Transform t = instance.transform;
                if (useLocalPositionAndRotation)
                {
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                }
                else
                {
                    t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
            }
        }
    }
}
