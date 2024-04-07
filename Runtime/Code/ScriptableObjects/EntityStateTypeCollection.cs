using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    [CreateAssetMenu(fileName = "New EntityStateTypeCollection", menuName = "MSU/EntityStateTypeCollection")]
    public class EntityStateTypeCollection : ScriptableObject
    {
        public SerializableEntityStateType[] stateTypes = Array.Empty<SerializableEntityStateType>();

#if UNITY_EDITOR
        [ContextMenu("Add Selected Scripts")]
        private void AddSelectedScripts()
        {
            var monoscripts = UnityEditor.Selection.assetGUIDs.Select(UnityEditor.AssetDatabase.GUIDToAssetPath).Select(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>)
                .OfType<UnityEditor.MonoScript>();

            var entityStateTypes = monoscripts.Select(ms => ms.GetClass())
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EntityState)));

            stateTypes = entityStateTypes.Select(t => new  SerializableEntityStateType(t)).ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}