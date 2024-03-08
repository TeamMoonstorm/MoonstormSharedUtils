﻿using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU
{
    public interface IEquipmentContentPiece : IContentPiece<EquipmentDef>
    {
        NullableRef<GameObject> ItemDisplayPrefab { get; }
        bool Execute(EquipmentSlot slot);

        void OnEquipmentObtained(CharacterBody body);

        void OnEquipmentLost(CharacterBody body);
    }
}