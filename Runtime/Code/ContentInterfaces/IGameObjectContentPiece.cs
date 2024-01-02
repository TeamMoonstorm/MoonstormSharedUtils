using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    public interface IGameObjectContentPiece<T> : IContentPiece<GameObject> 
    {
        T Component { get; }
    }
}