using BepInEx.Configuration;
using UnityEngine;

namespace Moonstorm
{
    internal static class ConfigLoader
    {
        public static ConfigEntry<bool> EnableDebugFeatures;

        public static ConfigEntry<bool> EnableLoggingOfIDRS;

        public static ConfigEntry<KeyCode> InstantiateMaterialTester;

        internal static void Init(ConfigFile config)
        {
            EnableDebugFeatures = config.Bind<bool>("MoonstormSharedUtils :: Debug Features",
                                                    "Enable Debug",
                                                    false,
                                                    "Enables Debug features from Moonstorm Shared Utils");

            EnableLoggingOfIDRS = config.Bind<bool>("MoonstormSharedUtils :: IDRS",
                                                    "Log IDRS-Related names",
                                                    false,
                                                    "Setting this to true causes MSU to Log inportant KEY values from RoR2's IDRS system\n" +
                                                    "It'll Log:\n" +
                                                    "IDRS names\n" +
                                                    "Key Assets names (ItemDefs & Equipment Defs)\n" +
                                                    "Display Prefabs");

            InstantiateMaterialTester = config.Bind<KeyCode>("MoonstormSharedUtils :: Keybinds",
                                                             "Instantiate Material Tester",
                                                             KeyCode.Insert,
                                                             "Keybind used for instantiating the material tester.");
        }
    }
}
