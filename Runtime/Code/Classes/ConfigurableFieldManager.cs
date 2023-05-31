using BepInEx;
using BepInEx.Configuration;
using System;

namespace Moonstorm
{
    [Obsolete("Use Moonstorm.Config.ConfigSystem class instead")]
    public static class ConfigurableFieldManager
    {
        [Obsolete("Use ConfigSystem.AddMod() instead")]
        public static void AddMod(BaseUnityPlugin baseUnityPlugin)
        {
            ConfigSystem.AddMod(baseUnityPlugin);
        }

        [Obsolete("Use ConfigSystem.GetConfigFile() instead")]
        public static ConfigFile GetConfigFile(string configIdentifier)
        {
            return ConfigSystem.GetConfigFile(configIdentifier);
        }
    }
}
