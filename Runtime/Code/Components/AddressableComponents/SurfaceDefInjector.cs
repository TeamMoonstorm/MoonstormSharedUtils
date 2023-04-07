using RoR2;
using UnityEngine;

namespace Moonstorm.Components.Addressables
{
    /// <summary>
    /// Injects a surface def specified in <see cref="surfaceDefAddress"/> to all SurfaceDefProviders that are children of the attached game object
    /// </summary>
    [ExecuteAlways]
    public class SurfaceDefInjector : MonoBehaviour
    {
        [Tooltip("The surfaceDef address to load")]
        public string surfaceDefAddress;
        private SurfaceDef loadedSurfaceDef;

        private void Awake() => Refresh();
        private void OnEnable() => Refresh();
        private void OnDisable() => RemoveReferencesEditor();

        /// <summary>
        /// Reloads the surface def and updates the SurfaceDefProvider components found in children of the attached game object
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(surfaceDefAddress) || string.IsNullOrEmpty(surfaceDefAddress))
            {
                Debug.LogWarning($"Invalid address in {this}, address is null, empty, or white space");
                return;
            }

            loadedSurfaceDef = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SurfaceDef>(surfaceDefAddress).WaitForCompletion();

            if (!loadedSurfaceDef)
                return;

            loadedSurfaceDef.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.NotEditable;
            foreach (var provider in GetComponentsInChildren<SurfaceDefProvider>())
            {
                provider.surfaceDef = loadedSurfaceDef;
            }
        }

        private void RemoveReferencesEditor()
        {
            if (!Application.isEditor)
                return;

            foreach (SurfaceDefProvider provider in GetComponentsInChildren<SurfaceDefProvider>())
            {
                provider.surfaceDef = null;
            }
        }
    }
}
