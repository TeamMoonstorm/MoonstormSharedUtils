using Moonstorm.AddressableAssets;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using static RoR2.CombatDirector;

namespace Moonstorm
{
    /// <summary>
    /// Represents a Serialized version of a <see cref="CombatDirector.EliteTierDef"/>
    /// <para>Utilized by the <see cref="EliteTierDefBase"/> and the <see cref="EliteTierDefModuleBase"/></para>
    /// </summary>
    [CreateAssetMenu(fileName = "New SerializableEliteTierDef", menuName = "Moonstorm/Elites/SerializableEliteTierDef", order = 0)]
    public class SerializableEliteTierDef : ScriptableObject
    {
        [Tooltip("This multiplier is applied to the cost of the CharacterSpawnCard when the CombatDirector tries to spawn an elite from this tier")]
        public float costMultiplier;
        public AddressReferencedEliteDef[] elites = Array.Empty<AddressReferencedEliteDef>();
        [HideInInspector, Obsolete("Use \"elites\" instead")]
        public AddressableAssets.AddressableEliteDef[] eliteTypes = Array.Empty<AddressableEliteDef>();
        [Tooltip("Wether the combat director can select this tier if no eliteTypes are supplied")]
        public bool canSelectWithoutAvailableEliteDef;

        /// <summary>
        /// The created EliteTierDef from the serialized data of a <see cref="SerializableEliteTierDef"/> and it's tied <see cref="EliteTierDefBase"/>
        /// </summary>
        public EliteTierDef EliteTierDef { get; private set; }
        
        private void Awake()
        {
#if !UNITY_EDITOR
            Migrate();
#endif
        }

        [ContextMenu("Migrate to R2API.Addressables")]
        private void Migrate()
        {
            if(eliteTypes.Length > 0 && elites.Length == 0)
            {
                elites = eliteTypes.Select(x => (AddressReferencedEliteDef)x).ToArray();
            }
        }
        
        internal void Create()
        {
            EliteTierDef = new EliteTierDef
            {
                eliteTypes = elites.Select(x => x.Asset).ToArray(),
                costMultiplier = costMultiplier,
                canSelectWithoutAvailableEliteDef = canSelectWithoutAvailableEliteDef,
                isAvailable = (rules) => true,
            };
        }
    }
}
