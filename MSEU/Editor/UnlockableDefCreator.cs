using RoR2;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils
{
    public static class UnlockableDefCreator
    {
        [MenuItem("Assets/Create/RoR2/UnlockableDef")]
        public static void CreateUnlockableDef()
        {
            var unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (System.IO.Path.GetExtension(path) != "")
            {
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewUnlockableDef.asset");
            AssetDatabase.CreateAsset(unlockableDef, path);
            AssetDatabase.ImportAsset(path);
        }
    }
}