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
    /// <summary>
    /// Instantiates the prefab specified in <see cref="address"/>
    /// </summary>
    [ExecuteAlways]
    public class InstantiateAddressablePrefab : MonoBehaviour
    {
        [Tooltip("The address to use to load the prefab")]
        [SerializeField] private string address;
        [Tooltip("When the prefab is instantiated, and this is true, the prefab's position and rotation will be set to 0")]
        [SerializeField] private bool setPositionAndRotationToZero;
        [Tooltip("Wether the Refresh method will be called in the editor")]
        [SerializeField] private bool refreshInEditor;
        [SerializeField, HideInInspector] private bool hasNetworkIdentity;

        /// <summary>
        /// The instantiated prefab
        /// </summary>
        public GameObject Instance => instance;
        private GameObject instance;

        private void Awake() => Refresh();
        private void OnEnable() => Refresh();
        private void OnDisable()
        {
            if (instance)
                DestroyImmediate(instance, true);
        }
        /// <summary>
        /// Destroys the instantiated object and re-instantiates using the prefab that's loaded via <see cref="address"/>
        /// </summary>
        public void Refresh()
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
            foreach(Transform t in instance.transform)
            {
                t.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
            }
            if(setPositionAndRotationToZero)
            {
                Transform t = instance.transform;
                t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
    }
}