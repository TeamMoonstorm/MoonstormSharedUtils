using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Components.Addressables
{
    [ExecuteAlways]
    public class InstantiateAddressablePrefab : MonoBehaviour
    {
        [SerializeField] private string address;
        [SerializeField] private bool setPositionAndRotationToZero;
        [SerializeField] private bool refreshInEditor;
        [SerializeField, HideInInspector] private bool hasNetworkIdentity;

        public GameObject Instance => instance;
        private GameObject instance;

        private void Awake() => Refresh();
        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            if (instance)
                DestroyImmediate(instance, true);
        }
        private void Refresh()
        {
            if (Application.isEditor && !refreshInEditor)
                return;

            if(instance)
            {
                DestroyImmediate(instance, true);
            }

            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrEmpty(address))
            {
                Debug.LogWarning($"Invalid address in {this}, address is null, empty, or white space");
                return;
            }

            GameObject prefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(address).WaitForCompletion();
            hasNetworkIdentity = prefab.GetComponent<NetworkIdentity>();

            if (hasNetworkIdentity && !Application.isEditor)
            {
                if(NetworkServer.active)
                {
                    instance = Instantiate(prefab, transform);
                    NetworkServer.Spawn(instance);
                }
            }
            else
            {
                instance = Instantiate(prefab, transform);
            }

            instance.hideFlags |= (HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable);
            if(setPositionAndRotationToZero)
            {
                Transform t = instance.transform;
                t.position = Vector3.zero;
                t.rotation = Quaternion.identity;
            }
        }
    }
}