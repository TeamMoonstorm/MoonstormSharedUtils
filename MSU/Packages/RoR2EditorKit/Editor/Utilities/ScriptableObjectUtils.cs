using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoR2EditorKit.Utilities
{
    /// <summary>
    /// Wrapper class of ThunderKit.Core.Utilities.ScriptableHelper
    /// </summary>
    public static class ScriptableObjectUtils
    {
        /// <summary>
        /// Creates and Saves a ScriptableObject of Type T allowing the user the input the name of the new asset, or cancel by pressing escape
        /// The asset will be created in one of the folowing:
        ///     The selected folder
        ///     The containing folder of a selected asset
        ///     The Assets folder if there is no selection in the Project window
        /// </summary>
        /// <typeparam name="T">Type of ScriptableObject to create</typeparam>
        /// <param name="afterCreated">Delegate to run after asset creation process has completed</param>
        public static void CreateNewScriptableObject<T>(Action<T> afterCreated = null) where T : ScriptableObject
        {
            ThunderKit.Core.Utilities.ScriptableHelper.SelectNewAsset<T>(afterCreated);
        }

        /// <summary>
        /// Creates and Saves a ScriptableObject of Type t
        /// The asset will be created in one of the folowing:
        ///     The selected folder
        ///     The containing folder of a selected asset
        ///     The Assets folder if there is no selection in the Project window
        /// </summary>
        /// <param name="t">Type of ScriptableObject to create</param>
        /// <param name="overrideName">Delegate which returns a string to be assigned as the name of the new asset</param>
        public static void CreateNewScriptableObject(Type t, Func<string> overrideName = null)
        {
            ThunderKit.Core.Utilities.ScriptableHelper.SelectNewAsset(t, overrideName);
        }

        /// <summary>
        /// if an Asset of Type T does not exist at assetPath, creates and saves a new asset of Type T
        /// </summary>
        /// <typeparam name="T">Type of ScriptableObject to create</typeparam>
        /// <param name="assetPath">Path to ScriptableObject</param>
        /// <param name="initializer">Delegate to run after asset creation process has completed</param>
        /// <returns>Created ScriptableObject</returns>
        public static T EnsureScriptableObjectExists<T>(string assetPath, Action<T> initializer = null) where T : ScriptableObject
        {
            return ThunderKit.Core.Utilities.ScriptableHelper.EnsureAsset<T>(assetPath, initializer);
        }

        /// <summary>
        /// if an Asset of Type t does not exist at assetPath, creates and saves a new asset of Type t
        /// </summary>
        /// <param name="assetPath">Path to ScriptableObject</param>
        /// <param name="t">Type of ScriptableObject to create</typeparam>
        /// <param name="initializer">Delegate to run after asset creation process has completed</param>
        /// <returns>Created ScriptableObject</returns>
        public static object EnsureScriptableObjectExists(string assetPath, Type type, Action<object> initializer = null)
        {
            return ThunderKit.Core.Utilities.ScriptableHelper.EnsureAsset(assetPath, type, initializer);
        }
    }
}
