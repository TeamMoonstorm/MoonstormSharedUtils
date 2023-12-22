using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public interface IItemContentPiece : IContentPiece<ItemDef>
    {
        NullableRef<GameObject> ItemDisplayPrefab { get; }
    }
}
