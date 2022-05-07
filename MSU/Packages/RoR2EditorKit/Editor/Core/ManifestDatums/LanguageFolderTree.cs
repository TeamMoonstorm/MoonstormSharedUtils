using System;
using ThunderKit.Core.Manifests;
using UnityEngine;

namespace RoR2EditorKit.Core.ManifestDatums
{
    /// <summary>
    /// A struct that represents a Language folder
    /// </summary>
    [Serializable]
    public struct LanguageFolder
    {
        /// <summary>
        /// The name of the language, IE: "en", "es-419"
        /// </summary>
        public string languageName;
        /// <summary>
        /// The .JSon/.TxT files for the language
        /// </summary>
        public TextAsset[] languageFiles;
    }

    /// <summary>
    /// Manifest datum for the pipeline StageLanguageFiles
    /// </summary>
    public class LanguageFolderTree : ManifestDatum
    {
        /// <summary>
        /// The name of the root folder
        /// </summary>
        public string rootFolderName;
        /// <summary>
        /// A collection of language folders
        /// </summary>
        public LanguageFolder[] languageFolders;
    }
}
