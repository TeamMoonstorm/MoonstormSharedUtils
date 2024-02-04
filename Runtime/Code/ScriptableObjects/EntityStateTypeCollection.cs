using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public class EntityStateTypeCollection : ScriptableObject
    {
        public SerializableEntityStateType[] stateTypes = Array.Empty<SerializableEntityStateType>();
    }
}