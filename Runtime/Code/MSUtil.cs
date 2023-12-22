using UnityEngine;
using R2API.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using RoR2;
using RoR2.ExpansionManagement;
using BepInEx.Configuration;
using System.Reflection;

namespace MSU
{
    public static class MSUtil
    {
        public static bool HolyDLLInstalled => IsModInstalled("xyz.yekoc.Holy");
        private static Run currentRun;
        private static ExpansionDef[] currentRunExpansionDefs = Array.Empty<ExpansionDef>();
        private static FieldInfo _configEntryTypedValueField;

        public static bool IsModInstalled(string GUID)
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);
        }

        public static float InverseHyperbolicScaling(float baseValue, float additionalValue, float maxValue, int itemCount)
        {
            return baseValue + (maxValue - baseValue) * (1 - 1 / (1 + additionalValue * (itemCount - 1)));
        }

        public static void DestroyImmediateSafe(UnityEngine.Object obj, bool allowDestroyingAssets = false)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(obj, allowDestroyingAssets);
#else
            GameObject.Destroy(obj);
#endif
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
        public static void AddToArraySafe<T>(this R2APISerializableContentPack contentPack, ref T[] contentArray, T asset) where T : class
        {
            if(contentArray.Contains(asset))
            {
#if DEBUG
                MSULog.Error($"Cannot add {asset} to content array of type {typeof(T).Name} to the content pack {contentPack.name} because its already in the array.");
#endif
                return;
            }

            if(asset is UnityEngine.Object obj)
            {
                if(obj.name.IsNullOrWhiteSpace())
                {
#if DEBUG
                    MSULog.Warning("Asset is of type UnityEngine.Object, but it has no name, setting a generic name.");
#endif
                    obj.name = $"{asset.GetType().Name}_{contentArray.Length}";
                }
            }

            HG.ArrayUtils.ArrayAppend(ref contentArray, asset);
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

        public static int GetItemCount(this CharacterBody body, ItemDef itemDef)
        {
            return body.inventory == null ? 0 : body.inventory.GetItemCount(itemDef);
        }

        public static int GetItemCount(this CharacterBody body, ItemIndex index)
        {
            return body.inventory == null ? 0 : body.inventory.GetItemCount(index);
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

        public static void SetValueWithoutNotifying<T>(this ConfigEntry<T> entry, T newValue)
        {
            _configEntryTypedValueField.SetValue(entry, newValue);
            entry.ConfigFile.Save();
        }

        public static bool IsAvailable(this ItemDef itemDef)
        {
            if (!Run.instance)
                return false;

            Run run = Run.instance;

            var unlockableDef = itemDef.unlockableDef;
            if (unlockableDef && !run.IsUnlockableUnlocked(unlockableDef))
                return false;

            var expansionDef = itemDef.requiredExpansion;
            if (expansionDef && !run.IsExpansionEnabled(expansionDef))
                return false;

            return true;
        }

        public static bool IsAvailable(this EquipmentDef equipmentDef)
        {
            if (!Run.instance)
                return false;

            Run run = Run.instance;

            var unlockableDef = equipmentDef.unlockableDef;
            if (unlockableDef && !run.IsUnlockableUnlocked(unlockableDef))
                return false;

            var expansionDef = equipmentDef.requiredExpansion;
            if (expansionDef && !run.IsExpansionEnabled(expansionDef))
                return false;

            return true;
        }

        static MSUtil()
        {
            Type configEntryType = typeof(ConfigEntry<object>);
            Type genericType = configEntryType.GetGenericTypeDefinition();
            _configEntryTypedValueField = genericType.GetField("_typedValue", BindingFlags.Instance | BindingFlags.NonPublic);
        }
#endregion
    }
}