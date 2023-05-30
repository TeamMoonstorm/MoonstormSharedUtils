namespace Moonstorm.EditorUtils.Importers
{
    using TKPriority = ThunderKit.Common.Constants.Priority;
    public static class Constants
    {
        public static class Priority
        {
            public const int WWiseBlacklister = TKPriority.AssemblyImport + 225_000;
            public const int InstallRiskOfOptions = TKPriority.AddressableCatalog - 165_000;
            public const int InstallFPTS = TKPriority.AddressableCatalog - 160_000;
        }
    }
}