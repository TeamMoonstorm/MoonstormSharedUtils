using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit
{
    public static class Util
    {
        public static IEnumerable<T> FindAssetsByType<T>(string assetNameFilter = null) where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids;
            if (assetNameFilter != null)
                guids = AssetDatabase.FindAssets($"{assetNameFilter} t:{typeof(T).Name}", null);
            else
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", null);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    assets.Add(asset);
            }
            return assets;
        }

        public static T FindAssetByType<T>(string assetNameFilter = null) where T : UnityEngine.Object
        {
            T asset;
            string[] guids;

            if (assetNameFilter != null)
                guids = AssetDatabase.FindAssets($"{assetNameFilter} t{typeof(T).Name}", null);
            else
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", null);

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids.First()));
        }

        public static Object CreateAssetAtSelectionPath(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{asset.name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();

            return asset;
        }

        public static GameObject CreatePrefabAtSelectionPath(GameObject asset)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{asset.name}.prefab");
            return PrefabUtility.SaveAsPrefabAsset(asset, path);
        }
        public static void AddTransformToParent(Transform child, Transform parent)
        {
            child.parent = parent;
            child.position = Vector3.zero;
        }
        public static void AddTransformToParent(Transform child, Transform parent, Vector3 pos)
        {
            child.parent = parent;
            child.position = pos;
        }
        public static void AddTransformToParent(GameObject child, GameObject parent)
        {
            child.transform.parent = parent.transform;
            child.transform.position = Vector3.zero;
        }
        public static void AddTransformToParent(GameObject child, GameObject parent, Vector3 pos)
        {
            child.transform.parent = parent.transform;
            child.transform.position = pos;
        }

        public static GameObject CreateGenericPrefab(string prefix, string name, Mesh mesh, Material material)
        {
            var prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = $"mdl{name}";
            var prefabMeshFilter = prefab.GetComponent<MeshFilter>();
            prefabMeshFilter.sharedMesh = mesh;
            var prefabMeshRenderer = prefab.GetComponent<MeshRenderer>();
            prefabMeshRenderer.sharedMaterial = material;

            return prefab;
        }
    }
}
