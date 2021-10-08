using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moonstorm.Utilities
{
    public static class MSUtil
    {
        /// <summary>
        /// Checks if a mod is installed in the bepinex chainloader
        /// </summary>
        /// <param name="GUID">The GUID of the mod to check.</param>
        /// <returns>True if installed, false otherwise.</returns>
        public static bool IsModInstalled(string GUID)
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);
        }
    }
}
