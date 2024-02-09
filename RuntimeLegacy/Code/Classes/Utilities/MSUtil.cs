using RoR2;
using RoR2.Audio;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Moonstorm
{
    public static class MSUtil
    {
        public static bool HolyDLLInstalled => IsModInstalled("xyz.yekoc.Holy");

        public static bool DebugToolkitInstalled => IsModInstalled("iHarbHD.DebugToolkit");

        private static Run currentRun;
        private static ExpansionDef[] currentRunExpansionDefs = Array.Empty<ExpansionDef>();
        private static Dictionary<AssemblyName, Type[]> assemblyToTypesCached = new Dictionary<AssemblyName, Type[]>();
        public static bool IsModInstalled(string GUID)
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);
        }

        public static float InverseHyperbolicScaling(float baseValue, float additionalValue, float maxValue, int itemCount)
        {
            return baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)));
        }

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

        public static ExpansionDef[] GetEnabledExpansions(this Run run)
        {
            if (currentRun == run)
            {
                return currentRunExpansionDefs;
            }

            currentRun = run;
            currentRunExpansionDefs = ExpansionCatalog.expansionDefs.Where(x => run.IsExpansionEnabled(x)).ToArray();
            return currentRunExpansionDefs;
        }

#if DEBUG
        public static void InvokeCommand(string commandName, params string[] arguments)
        {
            if (!RoR2.Console.instance)
                return;

            var args = arguments.ToList();
            var consoleUser = new RoR2.Console.CmdSender();
            consoleUser.networkUser = NetworkUser.instancesList.FirstOrDefault();
            consoleUser.localUser = consoleUser.networkUser ? consoleUser.networkUser.localUser : null;
            RoR2.Console.instance.RunCmd(consoleUser, commandName, args);
        }
#endif

        #region Extensions
        public static void Play(this NetworkSoundEventDef eventDef, Vector3 pos, bool transmit = true)
        {
            if (eventDef.index == NetworkSoundEventIndex.Invalid)
            {
                MSULog.Warning($"{eventDef} has an invalid network sound event index.");
                return;
            }
            EffectManager.SimpleSoundEffect(eventDef.index, pos, transmit);
        }

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

        public static bool RemoveIfInCollection<T>(this ICollection<T> collection, T entry)
        {
            if (!collection.Contains(entry))
                return false;
            collection.Remove(entry);
            return true;
        }

        public static int GetItemCount(this CharacterBody body, ItemDef itemDef)
        {
            return body.inventory == null ? 0 : body.inventory.GetItemCount(itemDef);
        }

        public static int GetItemCount(this CharacterBody body, ItemIndex index)
        {
            return body.inventory == null ? 0 : body.inventory.GetItemCount(index);
        }

        public static Type[] GetTypesSafe(this Assembly assembly)
        {
            if(assemblyToTypesCached.TryGetValue(assembly.GetName(), out var type))
            {
                return type;
            }
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException re)
            {
                types = re.Types.Where(t => t != null).ToArray();
            }
            assemblyToTypesCached.Add(assembly.GetName(), types);
            return types;
        }

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        public static string NicifyString(string text)
        {
            string origName = new string(text.ToCharArray());
            try
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                List<char> nameAsChar = null;
                if (text.StartsWith("m_", System.StringComparison.OrdinalIgnoreCase) || text.StartsWith("k_", System.StringComparison.OrdinalIgnoreCase))
                {
                    nameAsChar = text.Substring("m_".Length).ToList();
                }
                else
                {
                    nameAsChar = text.ToList();
                }

                while (nameAsChar.First() == '_')
                {
                    nameAsChar.RemoveAt(0);
                }
                List<char> newText = new List<char>();
                for (int i = 0; i < nameAsChar.Count; i++)
                {
                    char character = nameAsChar[i];
                    if (i == 0)
                    {
                        if (char.IsLower(character))
                        {
                            newText.Add(char.ToUpper(character));
                        }
                        else
                        {
                            newText.Add(character);
                        }
                        continue;
                    }

                    if (char.IsUpper(character))
                    {
                        newText.Add(' ');
                        newText.Add(character);
                        continue;
                    }
                    newText.Add(character);
                }
                return new String(newText.ToArray());
            }
            catch (Exception e)
            {
                MSULog.Error($"Failed to nicify {origName}: {e}");
                return origName;
            }
        }

        public static IEnumerable<T> GetValues<T>(this WeightedSelection<T> selection)
        {
            return selection.choices.Select(x => x.value).Where(x => x != null);
        }

        public static T AsValidOrNull<T>(this T obj) where T : UnityEngine.Object
        {
            return obj ? obj : null;
        }

        public static void DestroyImmediateSafe(UnityEngine.Object obj, bool allowDestroyingAssets = false)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(obj, allowDestroyingAssets);
#else
            GameObject.Destroy(obj);
#endif
        }
        #endregion
    }
}
