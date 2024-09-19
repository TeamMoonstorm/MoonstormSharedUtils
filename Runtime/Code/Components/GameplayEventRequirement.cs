using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// Component used to check if a <see cref="MSU.GameplayEvent"/> can spawn when <see cref="GameplayEventManager.SpawnGameplayEvent(GameplayEventManager.GameplayEventSpawnArgs)"/> is used.
    /// <br>This component is intended to be inherited if more checks are desired.</br>
    /// </summary>
    [RequireComponent(typeof(GameplayEvent))]
    public class GameplayEventRequirement : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="MSU.GameplayEvent"/> that's tied to this GameplayEventRequirement
        /// </summary>
        public GameplayEvent gameplayEvent { get; private set; }

        [Tooltip("At least this amount of stages should be completed before this GameplayEvent can spawn")]
        public int minimumStageCompletions = -1;
        [Tooltip("At least someone in the player team needs to have this unlockable unlocked before this GameplayEvent can spawn")]
        public AddressReferencedUnlockableDef requiredUnlockable;
        [Tooltip("None of the players should have this unlockable unlocked before this GameplayEvent can spawn")]
        public AddressReferencedUnlockableDef forbiddenUnlockable;
        [Tooltip("These ExpansionDefs need to be enabled in the run for this GameplayEvent to spawn")]
        public List<AddressReferencedExpansionDef> requiredExpansions;

        [Tooltip("The teleporter's charge needs to be bellow this value for a GameplayEvent to spawn")]
        [Range(0, 1f)]
        public float maximumTeleportCharge = 0.25f;

        /// <summary>
        /// Base method for <see cref="GameplayEventRequirement"/>
        /// <br>Call the base method so <see cref="gameplayEvent"/> gets populated correctly.</br>
        /// </summary>
        protected virtual void Awake()
        {
            gameplayEvent = GetComponent<GameplayEvent>();
        }

        /// <summary>
        /// Checks if the GameplayEvent attached to this GameplayEventRequirement can spawn.
        /// <br>By default it checks for StageCompletion count, required and forbidden unlockables, <see cref="RoR2.ExpansionManagement.ExpansionDef"/>s and the Teleporter's charge</br>
        /// </summary>
        /// <returns>True if the GameplayEvent can spawn, false otherwise.</returns>
        public virtual bool IsAvailable()
        {
            if (!Run.instance)
                return false;

            return CheckStageCompletions() && CheckUnlockables() && CheckExpansions() && CheckTeleporterCharge();
        }

        /// <summary>
        /// Checks if the total count of stages cleared surpasses the <see cref="minimumStageCompletions"/> number.
        /// </summary>
        /// <returns>True if <see cref="minimumStageCompletions"/> is -1 or if its less than the total count of stages cleared.</returns>
        protected bool CheckStageCompletions()
        {
            if (!Run.instance)
                return false;

            if (minimumStageCompletions == -1)
                return true;

            return Run.instance.stageClearCount > minimumStageCompletions;
        }

        /// <summary>
        /// Checks if the unlockable requirement is met for this GameplayEvent
        /// </summary>
        /// <returns>True if all Unlockable requirements are met, false otherwise.</returns>
        protected bool CheckUnlockables()
        {
            bool requiredUnlockableUnlocked = !requiredUnlockable || Run.instance.IsUnlockableUnlocked(requiredUnlockable);
            bool forbiddenUnlockableUnlocked = forbiddenUnlockable && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockable);

            return requiredUnlockableUnlocked && !forbiddenUnlockableUnlocked;
        }

        /// <summary>
        /// Checks if all the ExpansionDefs specified in <see cref="requiredExpansions"/> are enabled.
        /// </summary>
        /// <returns>True if all the expansions are enabled, false otherwise.</returns>
        protected bool CheckExpansions()
        {
            foreach (AddressReferencedExpansionDef expansionDef in requiredExpansions)
            {
                if (!expansionDef)
                    continue;

                if (!Run.instance.IsExpansionEnabled(expansionDef))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the Teleporter's charge fraction is less than the <see cref="maximumTeleportCharge"/>
        /// </summary>
        /// <returns>True if there is no <see cref="TeleporterInteraction.instance"/>, or if <see cref="TeleporterInteraction.chargeFraction"/> is less than <see cref="maximumTeleportCharge"/></returns>
        protected bool CheckTeleporterCharge()
        {
            if (!TeleporterInteraction.instance)
                return true;

            return TeleporterInteraction.instance.chargeFraction < maximumTeleportCharge;
        }
    }
}