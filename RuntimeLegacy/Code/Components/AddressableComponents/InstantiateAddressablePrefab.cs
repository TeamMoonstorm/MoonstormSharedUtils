using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Components.Addressables
{

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
            throw new System.NotImplementedException();
        }

        public void Refresh()
        {
            throw new System.NotImplementedException();
        }
    }
}
