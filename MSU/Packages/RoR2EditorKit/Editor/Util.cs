using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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

        public static void CreateNewScriptableObject<T>(Func<string> overrideName = null, Action<T> afterCreated = null) where T : ScriptableObject
        {
            ThunderKit.Core.ScriptableHelper.SelectNewAsset<T>(overrideName, afterCreated);
        }

        public static void CreateNewScriptableObject(Type t, Func<string> overrideName = null)
        {
            ThunderKit.Core.ScriptableHelper.SelectNewAsset(t, overrideName);
        }

        public static T EnsureScriptableObjectExists<T>(string assetPath, Action<T> initializer = null) where T : ScriptableObject
        {
            return ThunderKit.Core.ScriptableHelper.EnsureAsset<T>(assetPath, initializer);
        }

        public static object EnsureScriptableObjectExists(string assetPath, Type type, Action<object> initializer = null)
        {
            return ThunderKit.Core.ScriptableHelper.EnsureAsset(assetPath, type, initializer);
        }

        public static void UpdateNameOfObject(Object obj)
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), obj.name);
        }

        #region extensions
        public static bool IsNullOrEmptyOrWhitespace(this string text)
        {
            return (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text));
        }

        /// <summary>
        /// Sets the parent of this transform and sets position to a specified value
        /// </summary>
        /// <param name="parent">The parent transform</param>
        /// <param name="pos">The position of the child</param>
        public static void SetParent(this Transform t, Transform parent, Vector3 pos)
        {
            t.SetParent(parent, pos);
        }

        /// <summary>
        /// Sets the parent of a gameObject to another GameObject, and sets position to 0
        /// </summary>
        /// <param name="parent">The parent GameObject</param>
        public static void SetParent(this GameObject go, GameObject parent)
        {
            go.transform.SetParent(parent.transform, Vector3.zero);
        }

        /// <summary>
        /// Sets the parent of a gameObject to another GameObject, and sets position to a specified value
        /// </summary>
        /// <param name="parent">The parent GameObject</param>
        /// <param name="pos">The position of the Child</param>
        public static void SetParent(this GameObject go, GameObject parent, Vector3 pos)
        {
            go.transform.SetParent(parent.transform, pos);
        }

        public static SerializedProperty GetBindedProperty(this ObjectField objField, SerializedObject objectBound)
        {
            if (objField.bindingPath.IsNullOrEmptyOrWhitespace())
                throw new NullReferenceException($"{objField} doesnot have a bindingPath set");

            return objectBound.FindProperty(objField.bindingPath);
        }

        public static void TryRemoveFromParent(this VisualElement element)
        {
            if(element != null && element.parent != null)
            {
                element.parent.Remove(element);
            }
        }

        /// <summary>
        /// Quick method to set the ObjectField's object type
        /// </summary>
        /// <typeparam name="TObj">The type of object to set</typeparam>
        /// <param name="objField">The object field</param>
        public static void SetObjectType<T>(this ObjectField objField) where T : UnityEngine.Object
        {
            objField.objectType = typeof(T);
        }
        #endregion
    }
}
