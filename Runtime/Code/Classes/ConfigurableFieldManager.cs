using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Loaders;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

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
