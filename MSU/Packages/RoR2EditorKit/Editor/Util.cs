using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit
{
    /// <summary>
    /// Class holding various utility methods for interacting with the editor and the asset database
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Finds all assets of Type T
        /// </summary>
        /// <typeparam name="T">The Type of asset to find</typeparam>
        /// <param name="assetNameFilter">A filter to narrow down the search results</param>
        /// <returns>An IEnumerable of all the Types found inside the AssetDatabase.</returns>
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

        /// <summary>
        /// Finds an asset of Type T
        /// </summary>
        /// <typeparam name="T">The Type of asset to find</typeparam>
        /// <param name="assetNameFilter">A filter to narrow down the search results</param>
        /// <returns>The asset found</returns>
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

        /// <summary>
        /// Creates a generic asset at the currently selected folder
        /// </summary>
        /// <param name="asset">The asset to create</param>
        /// <returns>The Created asset</returns>
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

        /// <summary>
        /// Creates a prefab at the currently selected folder
        /// </summary>
        /// <param name="asset">The prefab to create</param>
        /// <returns>The newely created prefab in the AssetDatabase</returns>
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

        /// <summary>
        /// Adds a transform to a parent, and sets the child's position to 0
        /// </summary>
        /// <param name="child">The child transform</param>
        /// <param name="parent">The parent transform</param>
        public static void AddTransformToParent(Transform child, Transform parent)
        {
            child.parent = parent;
            child.position = Vector3.zero;
        }

        /// <summary>
        /// Adds a transform to a parent, and sets the child's position to a specified value
        /// </summary>
        /// <param name="child">The child transform</param>
        /// <param name="parent">The parent transform</param>
        /// <param name="pos">The position of the child</param>
        public static void AddTransformToParent(Transform child, Transform parent, Vector3 pos)
        {
            child.parent = parent;
            child.position = pos;
        }

        /// <summary>
        /// Adds a GameObject to a parent, and sets the child's position to zero
        /// </summary>
        /// <param name="child">The child GameObject</param>
        /// <param name="parent">The parent GameObject</param>
        public static void AddTransformToParent(GameObject child, GameObject parent)
        {
            child.transform.parent = parent.transform;
            child.transform.position = Vector3.zero;
        }

        /// <summary>
        /// Adds a GameObject to a parent, and sets the child's position to a specified value
        /// </summary>
        /// <param name="child">The child GameObject</param>
        /// <param name="parent">The parent GameObject</param>
        /// <param name="pos">The position of the Child</param>
        public static void AddTransformToParent(GameObject child, GameObject parent, Vector3 pos)
        {
            child.transform.parent = parent.transform;
            child.transform.position = pos;
        }

        /// <summary>
        /// Creates a generic game object ready to be transformed into a prefab
        /// </summary>
        /// <param name="name">The name of the game object</param>
        /// <param name="mesh">The mesh to use for the game object</param>
        /// <param name="material">The material to use for the game object</param>
        /// <returns>The created game object</returns>
        public static GameObject CreateGenericPrefab(string name, Mesh mesh, Material material)
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
