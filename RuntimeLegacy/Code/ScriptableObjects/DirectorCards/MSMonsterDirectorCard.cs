﻿using R2API;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static R2API.DirectorAPI;

namespace Moonstorm
{
    public class MSMonsterDirectorCard : CharacterSpawnCard
    {
        public struct PrefabComparer : IEqualityComparer<MSMonsterDirectorCard>
        {
            public bool Equals(MSMonsterDirectorCard x, MSMonsterDirectorCard y)
            {
                if (!x || !y)
                    return false;

                if (!x.prefab || !y.prefab)
                    return false;

                return x.prefab == y.prefab;
            }

            public int GetHashCode(MSMonsterDirectorCard obj)
            {
                if (!obj)
                    return -1;
                if (!obj.prefab)
                    return -1;

                return obj.prefab.GetHashCode();
            }
        }
        [Space(10)]
        [Header("Settings for DirectorAPI")]
        public AddressableDirectorCard addressReferencedDirectorCard = new AddressableDirectorCard();

        public MonsterCategory monsterCategory;

        public string customCategory;

        public float customCategoryWeight = 1;

        [EnumMask(typeof(R2API.DirectorAPI.Stage))]
        public R2API.DirectorAPI.Stage stages;

        public List<string> customStages = new List<string>();

        public List<AddressReferencedExpansionDef> requiredExpansionDefs = new List<AddressReferencedExpansionDef>();

        public DirectorCardHolder DirectorCardHolder
        {
            get
            {
                if (_directorCardHolder != null)
                {
                    return _directorCardHolder;
                }
                else
                {
                    _directorCardHolder = new DirectorCardHolder();
                    _directorCardHolder.Card = new DirectorCard
                    {
                        forbiddenUnlockableDef = addressReferencedDirectorCard.forbiddenUnlockableDef,
                        minimumStageCompletions = addressReferencedDirectorCard.minimumStageCompletions,
                        preventOverhead = addressReferencedDirectorCard.preventOverhead,
                        requiredUnlockableDef = addressReferencedDirectorCard.requiredUnlockableDef,
                        selectionWeight = addressReferencedDirectorCard.selectionWeight,
                        spawnCard = this,
                        spawnDistance = addressReferencedDirectorCard.spawnDistance
                    };
                    _directorCardHolder.MonsterCategory = monsterCategory;
                    _directorCardHolder.CustomMonsterCategory = customCategory;
                    _directorCardHolder.MonsterCategorySelectionWeight = customCategoryWeight;
                    _directorCardHolder.InteractableCategory = InteractableCategory.Invalid;
                    return _directorCardHolder;
                }
            }
            set
            {
                _directorCardHolder = value;
            }
        }
        private DirectorCardHolder _directorCardHolder = null;

        private new void Awake()
        {
            base.Awake();
            addressReferencedDirectorCard.spawnCard = this;
            customStages = customStages.Select(stageName => stageName.ToLowerInvariant()).ToList();
        }

        public virtual bool IsAvailable(ExpansionDef[] expansionDefs)
        {
            bool available = true;
            var reqExpansions = requiredExpansionDefs.Where(exp => exp.AssetExists);
            foreach (ExpansionDef ed in reqExpansions)
            {
                available = expansionDefs.Contains(ed);
            }
            return available;
        }
    }
}