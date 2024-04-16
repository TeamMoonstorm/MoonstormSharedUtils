using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            var monoscripts = UnityEditor.Selection.assetGUIDs.Select(UnityEditor.AssetDatabase.GUIDToAssetPath).Select(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>)
                .OfType<UnityEditor.MonoScript>();

            var entityStateTypes = monoscripts.Select(ms => ms.GetClass())
                .Where(t => t.IsSubclassOf(typeof(EntityState)));

            stateTypes = entityStateTypes.Select(t => new  SerializableEntityStateType(t)).Where(t => !stateTypes.Contains(t)).ToArray();

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}