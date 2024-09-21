using System;
using UnityEngine;

namespace Moonstorm.Components.Addressables
{
    public class CameraInstantiator : MonoBehaviour
    {
        public const string CAMERA_ADDRESS = "RoR2/Base/Core/Main Camera.prefab";
        public GameObject CameraInstance { get => _cameraInstance; private set => _cameraInstance = value; }
        [NonSerialized] private GameObject _cameraInstance;
        private void OnEnable() => throw new System.NotImplementedException();
        private void OnDisable() => throw new System.NotImplementedException();

        public void Refresh()
        {
            throw new System.NotImplementedException();
        }
    }
}