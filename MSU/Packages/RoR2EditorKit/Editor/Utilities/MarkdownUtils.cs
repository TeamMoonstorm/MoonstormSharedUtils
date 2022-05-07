using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;

namespace RoR2EditorKit.Utilities
{
    /// <summary>
    /// Generate markdown utilities for usage with Thunderkit's Markdown Element.
    /// </summary>
    public static class MarkdownUtils
    {
        /// <summary>
        /// Generates an AssetLink that points to the object specified.
        /// </summary>
        /// <param name="obj">The object to point towards</param>
        /// <returns>A string that represents the object's location, using the asset's GUID to ensure stability.</returns>
        public static string GenerateAssetLink(UnityEngine.Object obj) => GenerateAssetLink(obj.name, AssetDatabase.GetAssetPath(obj));

        /// <summary>
        /// Generates an AssetLink that points to a specified path.
        /// </summary>
        /// <param name="name">The name of the clickable link</param>
        /// <param name="path">The path to the asset, needs to be relative to the project</param>
        /// <returns>A string that represents the object's location, using the asset's GUID to ensure stability</returns>
        public static string GenerateAssetLink(string name, string path) => $"[{name}](assetlink://{AssetDatabase.AssetPathToGUID(path)})";
    }
}
