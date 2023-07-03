using UnityEngine;

namespace Moonstorm.Components.Addressables
{
    /// <summary>
    /// Instantiates the RoR2 main camera, which allows preview of post processing effects
    /// <para>Do not leave this on finalized builds, as it causes errors</para>
    /// </summary>
    [ExecuteAlways]
    public class CameraInstantiator : MonoBehaviour
    {
        public const string CAMERA_ADDRESS = "RoR2/Base/Core/Main Camera.prefab";
        [SerializeField, HideInInspector] private GameObject cameraInstance;
        private void OnEnable() => Refresh();
        private void OnDisable() => MSUtil.DestroyImmediateSafe(cameraInstance, true);

        /// <summary>
        /// Instantiates the camera or destroys the attached game object if the component is instantiated at runtime and not in the editor.
        /// </summary>
        public void Refresh()
        {
            if (Application.isPlaying && !Application.isEditor)
            {
                MSULog.Fatal($"Lingering camera injector in {gameObject}, Ensure that these scripts are NOT present on finalized builds!!!");
                Destroy(gameObject);
                return;
            }

            if (cameraInstance)
            {
                MSUtil.DestroyImmediateSafe(cameraInstance, true);
            }
            var go = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(CAMERA_ADDRESS).WaitForCompletion();
            cameraInstance = Instantiate(go, transform);
            cameraInstance.name = $"[EDITOR ONLY] {cameraInstance.name}";
            cameraInstance.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
            foreach (Transform t in cameraInstance.GetComponentsInChildren<Transform>())
            {
                t.gameObject.hideFlags = cameraInstance.hideFlags | HideFlags.HideInHierarchy;
            }
        }
    }
}