using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RoR2.CombatDirector;

namespace Moonstorm
{
    [CreateAssetMenu(fileName = "New SerializableEliteTierDef", menuName = "Moonstorm/Elites/SerializableEliteTierDef", order = 0)]
    public class SerializableEliteTierDef : ScriptableObject
    {
        public float costMultiplier;
        public AddressableAssets.AddressableEliteDef[] eliteTypes;
        public bool canSelectWithoutAvailableEliteDef;

        public EliteTierDef EliteTierDef { get; private set; }
        internal void Create()
        {
            EliteTierDef = new EliteTierDef
            {
                eliteTypes = eliteTypes.Select(aed => aed.Asset).ToArray(),
                costMultiplier = costMultiplier,
                canSelectWithoutAvailableEliteDef = canSelectWithoutAvailableEliteDef,
                isAvailable = (rules) => true,
            };
        }
    }
}
