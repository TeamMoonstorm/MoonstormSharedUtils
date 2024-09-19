using RoR2.Editor;
using System.IO;
using UnityEngine;

namespace MSU.Editor
{
    public static class MSUConstants
    {
        public const string MOONSTORM_SHARED_UTILS = "MoonstormSharedUtils";
        public const string PACKAGE_FOLDER_PATH = "Packages/teammoonstorm-moonstormsharedutils";
        public const string PACKAGE_NAME = "teammoonstorm-moonstormsharedutils";

        public const string MSU_CONTEXT_ROOT = "Assets/Create/MSU/";
        public const string MSU_SCRIPTABLE_ROOT = "Assets/MSU/";
        public const string MSU_MENU_ROOT = "Tools/MSU/";


        public static class AssetGUIDS
        {
            public static AssetGUID<TextAsset> xmlDoc = "ded440f4e5e23cd4a8bbfb38e5f13ebf";
            public static AssetGUID<Texture2D> msuIcon = "b4436cc7271d9f64da5496beb774571d";
        }

        internal static bool ValidateUXMLPath(this string path)
        {
            return path.Contains(PACKAGE_NAME);
        }

        public static class FolderPaths
        {
            private const string ASSETS = "Assets";
            private const string LIBRARY = "Library";
            private const string SCRIPT_ASSEMBLIES = "ScriptAssemblies";
            public static string libraryFolder
            {
                get
                {
                    var assetsPath = Application.dataPath;
                    var libFolder = assetsPath.Replace(ASSETS, LIBRARY);
                    return libFolder;
                }
            }

            public static string scriptAssembliesFolder
            {
                get
                {
                    return Path.Combine(libraryFolder, SCRIPT_ASSEMBLIES);
                }
            }
        }
    }
}
