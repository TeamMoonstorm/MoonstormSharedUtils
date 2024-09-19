using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Components.Addressables
{
    [ExecuteAlways]
    public class SurfaceDefInjector : MonoBehaviour
    {
        public string surfaceDefAddress;
        [NonSerialized]
        private SurfaceDef loadedSurfaceDef;

        private void OnEnable() => Refresh();
        private void OnDisable() => RemoveReferencesEditor();

        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(surfaceDefAddress) || string.IsNullOrEmpty(surfaceDefAddress))
            {
                MSULog.Warning($"Invalid address in {this}, address is null, empty, or white space");
                return;
            }

            loadedSurfaceDef = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SurfaceDef>(surfaceDefAddress).WaitForCompletion();

            if (!loadedSurfaceDef)
                return;
#if UNITY_EDITOR
            loadedSurfaceDef = Instantiate(loadedSurfaceDef);
#endif
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
