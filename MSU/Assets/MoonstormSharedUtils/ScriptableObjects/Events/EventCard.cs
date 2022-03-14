using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using R2API;
using RoR2;

namespace Moonstorm
{
    [CreateAssetMenu(menuName = "Cum/Cum")]
    public class EventCard : ScriptableObject
    {
        public EventIndex EventIndex { get; internal set; }

        [EnumMask(typeof(DirectorAPI.Stage))]
        public DirectorAPI.Stage stages;
    }
}
