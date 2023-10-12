using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm
{
    public interface IItemContentPiece : IContentPiece<ItemDef>
    {
        [CanBeNull]
        GameObject ItemDisplayPrefab { get; }
    }
}
