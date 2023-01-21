using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Components.Addressables
{
    [ExecuteAlways]
    public class CameraInstantiator : MonoBehaviour
    {
        public const string CAMERA_ADDRESS = "RoR2/Base/Core/Main Camera.prefab";
        [SerializeField, HideInInspector] private GameObject cameraInstance;
        private void Awake() => Refresh();
        private void OnEnable() => Refresh();
        private void OnDisable() => DestroyImmediate(cameraInstance, true);

        public void Refresh()
        {
            if (Application.isPlaying && !Application.isEditor)
            {
                Debug.LogError($"Lingering camera injector in {gameObject}, Ensure that these scripts are NOT present on finalized builds!!!");
                Destroy(gameObject);
                return;
            }

            if (cameraInstance)
            {
                DestroyImmediate(cameraInstance, true);
            }
            var go = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(CAMERA_ADDRESS).WaitForCompletion();
            cameraInstance = Instantiate(go, transform);
        }
    }
}