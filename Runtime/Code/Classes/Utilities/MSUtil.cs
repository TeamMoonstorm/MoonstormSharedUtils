using RoR2;
using RoR2.Audio;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// Utility methods used by MoonstormSharedUtils
    /// </summary>
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


        /// <summary>
        /// Calculates inverse hyperbolic scaling (diminishing) for the parameters passed in, and returns the result.
        /// <para>Uses the formula: baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)))</para>
        /// <para>Original code by KomradeSpectre</para>
        /// </summary>
        /// <param name="baseValue">The starting value of the function.</param>
        /// <param name="additionalValue">The value that is added per additional itemCount</param>
        /// <param name="maxValue">The maximum value that the function can possibly be.</param>
        /// <param name="itemCount">The amount of items/stacks that increments our function.</param>
        /// <returns>A float representing the inverse hyperbolic scaling of the parameters.</returns>
        public static float InverseHyperbolicScaling(float baseValue, float additionalValue, float maxValue, int itemCount)
        {
            return baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)));
        }

        /// <summary>
        /// Shorthand for playing a networked sound event def
        /// </summary>
        /// <param name="soundEventName">The name of the sound event</param>
        /// <param name="pos">Position at wich to play the sound</param>
        /// <param name="transmit"></param>
        public static void PlayNetworkedSFX(string soundEventName, Vector3 pos, bool transmit = true)
        {
            var soundID = NetworkSoundEventCatalog.FindNetworkSoundEventIndex(soundEventName);
            if (soundID == NetworkSoundEventIndex.Invalid)
            {
                MSULog.Warning($"Could not find sound event with name of {soundEventName}");
                return;
            }
            EffectManager.SimpleSoundEffect(soundID, pos, transmit);
        }

        #region Extensions
        public static T EnsureComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            var comp = obj.GetComponent<T>();
            if (!comp)
                comp = obj.AddComponent<T>();

            return comp;
        }

        public static bool AddIfNotInCollection<T>(this ICollection<T> collection, T entry)
        {
            if (collection.Contains(entry))
                return false;
            collection.Add(entry);
            return true;
        }

        public static bool RemoveIfNotInCollection<T>(this ICollection<T> collection, T entry)
        {
            if (!collection.Contains(entry))
                return false;
            collection.Remove(entry);
            return true;
        }
        #endregion
    }
}
