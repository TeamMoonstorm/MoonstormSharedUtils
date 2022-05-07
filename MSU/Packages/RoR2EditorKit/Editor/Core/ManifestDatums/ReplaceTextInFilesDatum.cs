using ThunderKit.Core.Manifests;
using UnityEngine;

namespace RoR2EditorKit.Core.ManifestDatums
{
    /// <summary>
    /// Manifest datum for the pipeline ReplaceTextInFiles
    /// </summary>
    public class ReplaceTextInFilesDatum : ManifestDatum
    {
        /// <summary>
        /// A collection of objects which text will be modified from
        /// </summary>
        public Object[] Objects;
    }
}
