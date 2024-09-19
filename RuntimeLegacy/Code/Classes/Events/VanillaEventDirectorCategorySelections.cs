namespace Moonstorm
{
    public static class VanillaEventDirectorCategorySelections
    {
        public static EventDirectorCategorySelection AbandonedAqueduct { get; } = Load(nameof(AbandonedAqueduct));

        public static EventDirectorCategorySelection AbyssalDepths { get; } = Load(nameof(AbyssalDepths));

        public static EventDirectorCategorySelection AphelianSanctuary { get; } = Load(nameof(AphelianSanctuary));

        public static EventDirectorCategorySelection ArtifactReliquary { get; } = Load(nameof(ArtifactReliquary));

        public static EventDirectorCategorySelection Commencement { get; } = Load(nameof(Commencement));

        public static EventDirectorCategorySelection DistantRoost { get; } = Load(nameof(DistantRoost));

        public static EventDirectorCategorySelection RallypointDelta { get; } = Load(nameof(RallypointDelta));

        public static EventDirectorCategorySelection ScorchedAcres { get; } = Load(nameof(ScorchedAcres));

        public static EventDirectorCategorySelection SiphonedForest { get; } = Load(nameof(SiphonedForest));

        public static EventDirectorCategorySelection SirensCall { get; } = Load(nameof(SirensCall));

        public static EventDirectorCategorySelection SkyMeadow { get; } = Load(nameof(SkyMeadow));

        public static EventDirectorCategorySelection SulphurPools { get; } = Load(nameof(SulphurPools));

        public static EventDirectorCategorySelection SunderedGrove { get; } = Load(nameof(SunderedGrove));

        public static EventDirectorCategorySelection ThePlanetarium { get; } = Load(nameof(ThePlanetarium));

        public static EventDirectorCategorySelection VoidCell { get; } = Load(nameof(VoidCell));

        public static EventDirectorCategorySelection VoidLocus { get; } = Load(nameof(VoidLocus));

        public static EventDirectorCategorySelection WetlandAspect { get; } = Load(nameof(WetlandAspect));

        public static EventDirectorCategorySelection TitanicPlains { get; } = Load(nameof(TitanicPlains));

        public static EventDirectorCategorySelection GildedCoast { get; } = Load(nameof(GildedCoast));

        public static EventDirectorCategorySelection AbandonedAqueductSimulacrum { get; } = Load(nameof(AbandonedAqueductSimulacrum));

        public static EventDirectorCategorySelection AbyssalDepthsSimulacrum { get; } = Load(nameof(AbyssalDepthsSimulacrum));

        public static EventDirectorCategorySelection AphelianSanctuarySimulacrum { get; } = Load(nameof(AphelianSanctuarySimulacrum));

        public static EventDirectorCategorySelection CommencementSimulacrum { get; } = Load(nameof(CommencementSimulacrum));

        public static EventDirectorCategorySelection RallypointDeltaSimulacrum { get; } = Load(nameof(RallypointDeltaSimulacrum));

        public static EventDirectorCategorySelection SkyMeadowSimulacrum { get; } = Load(nameof(SkyMeadowSimulacrum));

        public static EventDirectorCategorySelection TitanicPlainsSimulacrum { get; } = Load(nameof(TitanicPlainsSimulacrum));

        private static EventDirectorCategorySelection Load(string name) => MoonstormSharedUtils.MSUAssetBundle.LoadAsset<EventDirectorCategorySelection>($"edcs{name}");
    }
}
