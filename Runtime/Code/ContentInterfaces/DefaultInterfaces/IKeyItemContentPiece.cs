using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    public interface IKeyItemContentPiece : IContentPiece<KeyItemDef>
    {
        NullableRef<List<GameObject>> itemDisplayPrefabs { get; }
    }
}