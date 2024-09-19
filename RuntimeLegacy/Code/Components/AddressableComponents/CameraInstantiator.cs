using System;
using UnityEngine;

namespace Moonstorm.Components.Addressables
{
    [ExecuteAlways]
    public class CameraInstantiator : MonoBehaviour
    {
        public const string CAMERA_ADDRESS = "RoR2/Base/Core/Main Camera.prefab";
        public GameObject CameraInstance { get => _cameraInstance; private set => _cameraInstance = value; }
        [NonSerialized] private GameObject _cameraInstance;
        private void OnEnable() => Refresh();
        private void OnDisable() => MSUtil.DestroyImmediateSafe(CameraInstance, true);

        public void Refresh()
        {
            if (Application.isPlaying && !Application.isEditor)
            {
                MSULog.Fatal($"Lingering camera injector in {gameObject}, Ensure that these scripts are NOT present on finalized builds!!!");
                Destroy(gameObject);
                return;
            }

            if (CameraInstance)
            {
                MSUtil.DestroyImmediateSafe(CameraInstance, true);
            }
            var go = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(CAMERA_ADDRESS).WaitForCompletion();
            CameraInstance = Instantiate(go, transform);
            CameraInstance.name = $"[EDITOR ONLY] {CameraInstance.name}";
            CameraInstance.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
            foreach (Transform t in CameraInstance.GetComponentsInChildren<Transform>())
            {
                t.gameObject.hideFlags = CameraInstance.hideFlags | HideFlags.HideInHierarchy;
            }
            CameraInstance.hideFlags &= ~HideFlags.HideInHierarchy;
        }
    }
}