using EntityStates;
using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MSU
{
    /// <summary>
    /// An EntityStateTypeCollection is a ScriptableObject which is used to contain a collection of <see cref="SerializableEntityStateType"/>, which can be then added to ContentPacks.
    /// </summary>
    [CreateAssetMenu(fileName = "New EntityStateTypeCollection", menuName = "MSU/EntityStateTypeCollection")]
    public class EntityStateTypeCollection : ScriptableObject
    {
        [Tooltip("The StateTypes stored in this EntityStateTypeCollection.")]
        public SerializableEntityStateType[] stateTypes = Array.Empty<SerializableEntityStateType>();

#if UNITY_EDITOR
        [ContextMenu("Add Selected Scripts")]
        private void AddSelectedScripts()
        {
            var monoscripts = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>)
                .OfType<MonoScript>();

            var entityStateTypes = monoscripts.Select(ms => ms.GetClass())
                .Where(t => t.IsSubclassOf(typeof(EntityState)));

            stateTypes = stateTypes.Union(entityStateTypes.Select(t => new SerializableEntityStateType(t)).Where(t => !stateTypes.Contains(t))).ToArray();

            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Add Scripts found on this Directory and Children")]
        private void AddScriptsFoundOnThisDirectoryAndChildren()
        {
            var monoScripts = AssetDatabase.FindAssets("t:MonoScript", new string[] { System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(this))})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>)
                .OfType<MonoScript>();

            var entityStateTypes = monoScripts.Select(ms => ms.GetClass())
                .Where(t => t.IsSameOrSubclassOf(typeof(EntityState)));

            stateTypes = stateTypes.Union(entityStateTypes.Select(t => new SerializableEntityStateType(t))).ToArray();

            EditorUtility.SetDirty(this);
        }
#endif
    }
}