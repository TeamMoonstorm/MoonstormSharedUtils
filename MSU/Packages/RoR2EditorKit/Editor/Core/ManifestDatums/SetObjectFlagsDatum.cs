using ThunderKit.Core.Manifests;
using UnityEngine;

namespace RoR2EditorKit.Core.ManifestDatums
{
    /// <summary>
    /// Manifest datum for the pipeline SetObjectFlags
    /// </summary>
    public class SetObjectFlagsDatum : ManifestDatum
    {
        /// <summary>
        /// A collection of objects which flags will be modified
        /// </summary>
        public Object[] objects;
    }
}
