﻿using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    [InitializeOnLoad]
    internal static class CopyXMLDoc
    {
        static CopyXMLDoc()
        {
            if (ShouldCopy())
            {
                Debug.Log($"Copying over the MSU XML Doc");
                DoCopy();
            }
        }

        private static bool ShouldCopy()
        {
            var relativePath = AssetDatabase.GUIDToAssetPath("ded440f4e5e23cd4a8bbfb38e5f13ebf");
            var fullPath = Path.GetFullPath(relativePath);
            var fileName = Path.GetFileName(fullPath);
            var pathToCheck = Path.Combine(RoR2EditorKit.Constants.FolderPaths.ScriptAssembliesFolder, fileName);
            return !File.Exists(pathToCheck);
        }

        private static void DoCopy()
        {
            var relativePath = AssetDatabase.GUIDToAssetPath("ded440f4e5e23cd4a8bbfb38e5f13ebf");
            var sourcePath = Path.GetFullPath(relativePath);
            var fileName = Path.GetFileName(sourcePath);
            var destPath = Path.Combine(Constants.FolderPaths.ScriptAssembliesFolder, fileName);
            File.Copy(sourcePath, destPath, true);
        }
    }
}
