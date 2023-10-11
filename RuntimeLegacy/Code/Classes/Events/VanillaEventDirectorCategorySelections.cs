namespace Moonstorm
{
    /// <summary>
    /// A static class holding all the EventDirectorCategorySelections for the Vanilla stages.
    /// </summary>
    public static class VanillaEventDirectorCategorySelections
    {
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection AbandonedAqueduct { get; } = Load(nameof(AbandonedAqueduct));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection AbyssalDepths { get; } = Load(nameof(AbyssalDepths));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection AphelianSanctuary { get; } = Load(nameof(AphelianSanctuary));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection ArtifactReliquary { get; } = Load(nameof(ArtifactReliquary));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection Commencement { get; } = Load(nameof(Commencement));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection DistantRoost { get; } = Load(nameof(DistantRoost));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection RallypointDelta { get; } = Load(nameof(RallypointDelta));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection ScorchedAcres { get; } = Load(nameof(ScorchedAcres));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection SiphonedForest { get; } = Load(nameof(SiphonedForest));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection SirensCall { get; } = Load(nameof(SirensCall));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection SkyMeadow { get; } = Load(nameof(SkyMeadow));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection SulphurPools { get; } = Load(nameof(SulphurPools));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection SunderedGrove { get; } = Load(nameof(SunderedGrove));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection ThePlanetarium { get; } = Load(nameof(ThePlanetarium));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection VoidCell { get; } = Load(nameof(VoidCell));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection VoidLocus { get; } = Load(nameof(VoidLocus));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection WetlandAspect { get; } = Load(nameof(WetlandAspect));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection TitanicPlains { get; } = Load(nameof(TitanicPlains));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection GildedCoast { get; } = Load(nameof(GildedCoast));

        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection AbandonedAqueductSimulacrum { get; } = Load(nameof(AbandonedAqueductSimulacrum));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection AbyssalDepthsSimulacrum { get; } = Load(nameof(AbyssalDepthsSimulacrum));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection AphelianSanctuarySimulacrum { get; } = Load(nameof(AphelianSanctuarySimulacrum));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection CommencementSimulacrum { get; } = Load(nameof(CommencementSimulacrum));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection RallypointDeltaSimulacrum { get; } = Load(nameof(RallypointDeltaSimulacrum));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection SkyMeadowSimulacrum { get; } = Load(nameof(SkyMeadowSimulacrum));
        /// <summary>
        /// <inheritdoc cref="Load(string)"/>
        /// </summary>
        public static EventDirectorCategorySelection TitanicPlainsSimulacrum { get; } = Load(nameof(TitanicPlainsSimulacrum));
        /// <summary>
        /// Loads the EventDirecttorCategorySelection for the stage specified in the property name
        /// </summary>
        private static EventDirectorCategorySelection Load(string name) => MoonstormSharedUtils.MSUAssetBundle.LoadAsset<EventDirectorCategorySelection>($"edcs{name}");
    }
}
