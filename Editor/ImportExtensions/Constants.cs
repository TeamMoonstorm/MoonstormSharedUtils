namespace MSU.Editor.Importers
{
    using TKPriority = ThunderKit.Common.Constants.Priority;
    public static class Constants
    {
        public static class Priority
        {
            public const int WWISE_BLACKLISTER = TKPriority.AssemblyImport + 225_000;
            public const int INSTALL_DEPENDENCIES = TKPriority.AddressableCatalog - 165_000;
        }
    }
}