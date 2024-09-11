using RoR2.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor
{
    public static class MSUConstants
    {
        public const string MoonstormSharedUtils = nameof(MoonstormSharedUtils);
        public const string PackageFolderPath = "Packages/teammoonstorm-moonstormsharedutils";
        public const string PackageName = "teammoonstorm-moonstormsharedutils";

        public const string MSUContextRoot = "Assets/Create/MSU/";
        public const string MSUScriptableRoot = "Assets/MSU/";
        public const string MSUMenuRoot = "Tools/MSU/";


        public static class AssetGUIDS
        {
            public static AssetGUID<TextAsset> xmlDoc = "ded440f4e5e23cd4a8bbfb38e5f13ebf";
            public static AssetGUID<Texture2D> msuIcon = "b4436cc7271d9f64da5496beb774571d";
        }

        private static T Load<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        internal static bool ValidateUXMLPath(this string path)
        {
            return path.Contains(PackageName);
        }

        public static class FolderPaths
        {
            private const string assets = "Assets";
            private const string lib = "Library";
            private const string scriptAssemblies = "ScriptAssemblies";
            public static string LibraryFolder
            {
                get
                {
                    var assetsPath = Application.dataPath;
                    var libFolder = assetsPath.Replace(assets, lib);
                    return libFolder;
                }
            }

            public static string ScriptAssembliesFolder
            {
                get
                {
                    return Path.Combine(LibraryFolder, scriptAssemblies);
                }
            }
        }
    }
}
