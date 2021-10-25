namespace Moonstorm
{
    /// <summary>
    /// A Content Base Class for initializing an Elite Equipment
    /// <para>Inherits from EquipmentBase</para>
    /// </summary>
    public abstract class EliteEquipmentBase : EquipmentBase
    {
        /// <summary>
        /// Your Elite's MSEliteDef
        /// </summary>
        public abstract MSEliteDef EliteDef { get; set; }

        /// <summary>
        /// Data regarding Aspect Abilities. Will be removed soon.
        /// </summary>
        public abstract MSAspectAbilityDataHolder AspectAbilityData { get; set; }

        /// <summary>
        /// Initialize your Elite Equipment.
        /// <para>calling base.Initialize() heavily reccomended.</para>
        /// </summary>
        public override void Initialize()
        {
            if (MSUtil.IsModInstalled("com.TheMysticSword.AspectAbilities"))
            {
                if (AspectAbilityData != null)
                    RunAspectAbility();
                else
                    MSULog.LogD($"The EliteEquipment for {EliteDef.name} doesnt have an Aspect Ability Data asset associated with it.");
            }
        }

        private void RunAspectAbility()
        {
            AspectAbilities.AspectAbility ability = new AspectAbilities.AspectAbility
            {
                aiMaxUseDistance = AspectAbilityData.aiMaxUseDistance,
                aiHealthFractionToUseChance = AspectAbilityData.aiHealthFractionToUseChance,
                equipmentDef = AspectAbilityData.equipmentDef,
                onUseOverride = FireAction,
            };

            AspectAbilities.AspectAbilitiesPlugin.RegisterAspectAbility(ability);
        }
    }
}
