using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ExpansionManagement;

namespace MSU
{
    public class GameplayEventRequirement : MonoBehaviour
    {
        public int minimumStageCompletions = -1;
        public AddressReferencedUnlockableDef requiredUnlockable;
        public AddressReferencedUnlockableDef forbiddenUnlockable;
        public List<AddressReferencedExpansionDef> requiredExpansions;
        [Range(0, 1f)]
        public float maximumTeleportCharge = 0.25f;

        public virtual bool IsAvailable()
        {
            if (!Run.instance)
                return false;

            return CheckStageCompletions() && CheckUnlockables() && CheckExpansions() && CheckTeleporterCharge();
        }

        protected bool CheckStageCompletions()
        {
            if (minimumStageCompletions == -1)
                return true;

            return Run.instance.stageClearCount > minimumStageCompletions;
        }

        protected bool CheckUnlockables()
        {
            bool requiredUnlockableUnlocked = !requiredUnlockable || Run.instance.IsUnlockableUnlocked(requiredUnlockable);
            bool forbiddenUnlockableUnlocked = forbiddenUnlockable && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockable);

            return requiredUnlockableUnlocked && !forbiddenUnlockableUnlocked;
        }

        protected bool CheckExpansions()
        {
            foreach(AddressReferencedExpansionDef expansionDef in requiredExpansions)
            {
                if (!Run.instance.IsExpansionEnabled(expansionDef))
                    return false;
            }
            return true;
        }

        protected bool CheckTeleporterCharge()
        {
            if (!TeleporterInteraction.instance)
                return true;

            return TeleporterInteraction.instance.chargeFraction < maximumTeleportCharge;
        }
    }
}