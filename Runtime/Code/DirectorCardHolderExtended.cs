using RoR2;
using RoR2.ExpansionManagement;
using R2API;
using System;
using System.Linq;

namespace MSU
{
    /// <summary>
    /// A subclass of the <see cref="DirectorAPI.DirectorCardHolder"/>, the <see cref="DirectorCardHolderExtended"/> adds a new field for required ExpansionDefs and provides a custom IsAvailable() call. This is done to allow for multiple expansion requirements for interactables and monsters managed by the <see cref="CharacterModule"/> and <see cref="InteractableModule"/> respectively.
    /// </summary>
    public class DirectorCardHolderExtended : DirectorAPI.DirectorCardHolder
    {
        /// <summary>
        /// An array of <see cref="ExpansionDef"/>, ALL the ExpansionDefs provided here MUST be in the current run for this DirectorCard to be added to the existing DirectorCardCategorySelections.
        /// </summary>
        public ExpansionDef[] requiredExpansionDefs = Array.Empty<ExpansionDef>();

        /// <summary>
        /// Calls the underlying DirectorCard's IsAvailable method, alongside <see cref="ExpansionRequirementMet"/> and returns the computed logical AND value.
        /// </summary>
        /// <returns>True if both the Card's IsAvailable returns true AND <see cref="ExpansionRequirementMet"/> returns true, otherwise false.</returns>
        public bool IsAvailable()
        {
            return Card.IsAvailable() && ExpansionRequirementMet();
        }

        /// <summary>
        /// Returns True if ALL the ExpansionDefs inside <see cref="requiredExpansionDefs"/> are enabled in the current run.
        /// </summary>
        public bool ExpansionRequirementMet()
        {
            ExpansionDef[] runExpansions = Run.instance.GetEnabledExpansions();
            foreach (var expansionDef in requiredExpansionDefs)
            {
                if (!runExpansions.Contains(expansionDef))
                {
                    return false;
                }
            }
            return true;
        }
    }
}