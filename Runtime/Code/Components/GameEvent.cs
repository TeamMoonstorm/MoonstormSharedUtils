using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MSU
{
    public class GameplayEvent : NetworkBehaviour
    {
        public GameplayEventIndex GameplayEventIndex { get; internal set; }
    }
}