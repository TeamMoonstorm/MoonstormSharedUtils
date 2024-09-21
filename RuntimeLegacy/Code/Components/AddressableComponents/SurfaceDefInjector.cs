using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Components.Addressables
{
    public class SurfaceDefInjector : MonoBehaviour
    {
        public string surfaceDefAddress;
        [NonSerialized]
        private SurfaceDef loadedSurfaceDef;

        private void OnEnable() => Refresh();
        private void OnDisable() => RemoveReferencesEditor();

        public void Refresh()
        {
            throw new System.NotImplementedException();
        }

        private void RemoveReferencesEditor()
        {
            throw new System.NotImplementedException();
        }
    }
}
