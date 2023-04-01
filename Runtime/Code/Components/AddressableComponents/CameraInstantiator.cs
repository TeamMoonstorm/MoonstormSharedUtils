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
        private void Awake() => Refresh();
        private void OnEnable() => Refresh();
        private void OnDisable() => DestroyImmediate(cameraInstance, true);

        /// <summary>
        /// Instantiates the camera or destroys the attached game object if the component is instantiated at runtime and not in the editor.
        /// </summary>
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
            cameraInstance.name = $"[EDITOR ONLY] {cameraInstance.name}";
            cameraInstance.hideFlags |= (HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild);
            foreach (Transform t in cameraInstance.transform)
            {
                t.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }
        }
    }
}