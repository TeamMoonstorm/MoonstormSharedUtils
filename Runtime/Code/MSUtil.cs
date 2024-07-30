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
using System.Collections;

namespace MSU
{
    /// <summary>
    /// Utility methods by MSU
    /// </summary>
    public static class MSUtil
    {
        /// <summary>
        /// Wether or not DebugToolKit by iHarbHD is installed
        /// </summary>
        public static bool DebugToolkitInstalled => IsModInstalled("iHarbHD.DebugToolkit");
        /// <summary>
        /// Wether or not HolyDLL by RandomlyAwesome is installed
        /// </summary>
        public static bool HolyDLLInstalled => IsModInstalled("xyz.yekoc.Holy");
        private static Run currentRun;
        private static ExpansionDef[] currentRunExpansionDefs = Array.Empty<ExpansionDef>();
        private static FieldInfo _configEntryTypedValueField;

        /// <summary>
        /// A wrapper for accessing BepInEx's ChainLoader and checking if a specific GUID is within the ChainLoader's plugin infos
        /// </summary>
        /// <param name="GUID">The mod to check if its installed</param>
        /// <returns>True if the mod is installed, false otherwise</returns>
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
        /// A Wrapper for <see cref="UnityEngine.Object.DestroyImmediate(UnityEngine.Object)"/>, which calls DestroyImmediate in the Editor, and calls Regular Destroy when outside the Editor
        /// </summary>
        /// <param name="obj">The object to destroy</param>
        /// <param name="allowDestroyingAssets">Wether or not <paramref name="obj"/> can be destroyed, even if its an Asset</param>
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
        /// <summary>
        /// Returns an array of the enabled Expansions in a Run
        /// </summary>
        /// <returns>An array of the enabled <see cref="ExpansionDef"/>s</returns>
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

        /// <summary>
        /// Ensures that the component of type <typeparamref name="T"/> is in the specified GameObject.
        /// </summary>
        /// <typeparam name="T">The type of component to ensure it exists</typeparam>
        /// <param name="obj">The GameObject that'll have the component ensured</param>
        /// <returns>A new instance of T if it wasnt found in the GameObject, otherwise it returns an existing instance</returns>
        public static T EnsureComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            if(obj.TryGetComponent<T>(out var t))
            {
                return t;
            }
            return obj.AddComponent<T>();
        }

        /// <summary>
        /// Checks if a Body has a specific item
        /// </summary>
        /// <returns>True if the body has the specified item, false otherwise</returns>
        public static bool HasItem(this CharacterBody body, ItemDef def)
        {
            return body.HasItem(def.itemIndex);
        }

        /// <summary>
        /// Checks if a Body has a specific item
        /// </summary>
        /// <returns>True if the body has the specified item, false otherwise</returns>
        public static bool HasItem(this CharacterBody body, ItemIndex index)
        {
            return body.GetItemCount(index) > 0;
        }

        /// <summary>
        /// Attempts to obtain the item count of <paramref name="itemDef"/> from <paramref name="body"/>'s inventory.
        /// <para>In case the body does not have an Inventory, it'll return false.</para>
        /// </summary>
        /// <param name="body">The body to check it's item count</param>
        /// <param name="itemDef">The type of item to count</param>
        /// <param name="itemCount">An outgoing variable that'll have the item's count</param>
        /// <returns>True if the item count was retrieved succesfully, false otherwise</returns>
        public static bool TryGetItemCount(this CharacterBody body, ItemDef itemDef, out int itemCount)
        {
            return body.TryGetItemCount(itemDef.itemIndex, out itemCount);
        }

        /// <summary>
        /// Attempts to obtain the item count of <paramref name="index"/> from <paramref name="body"/>'s inventory.
        /// <para>In case the body does not have an Inventory, it'll return false.</para>
        /// </summary>
        /// <param name="body">The body to check it's item count</param>
        /// <param name="index">The type of item to count</param>
        /// <param name="itemCount">An outgoing variable that'll have the item's count</param>
        /// <returns>True if the item count was retrieved succesfully, false otherwise</returns>
        public static bool TryGetItemCount(this CharacterBody body, ItemIndex index, out int itemCount)
        {
            itemCount = body.GetItemCount(index);
            return itemCount > 0;
        }

        /// <summary>
        /// Obtains the item count of <paramref name="itemDef"/> from <paramref name="body"/>
        /// </summary>
        /// <param name="body">The body to check it's item count</param>
        /// <param name="itemDef">The type of item to count</param>
        /// <returns>The item's count, 0 if the body does not have an inventory</returns>
        public static int GetItemCount(this CharacterBody body, ItemDef itemDef)
        {
            return body.GetItemCount(itemDef.itemIndex);
        }

        /// <summary>
        /// Obtains the item count of <paramref name="index"/> from <paramref name="body"/>
        /// </summary>
        /// <param name="body">The body to check it's item count</param>
        /// <param name="index">The type of item to count</param>
        /// <returns>The item's count, 0 if the body does not have an inventory</returns>
        public static int GetItemCount(this CharacterBody body, ItemIndex index)
        {
            return body.inventory ? body.inventory.GetItemCount(index) : 0;
        }

        /// <summary>
        /// Deconstructs a KeyValuePair into two outgoing variables, useful for iterating dictionaries.
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        /// <summary>
        /// "Nicifies" a string into a readable format
        /// </summary>
        /// <param name="text">The string to "Nicify"</param>
        /// <returns>The "Nicified" string, if an exception is thrown it returns <paramref name="text"/></returns>
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

        /// <summary>
        /// Obtains the values of a WeightedSelection
        /// </summary>
        /// <typeparam name="T">The type enclosed in the Weighted Selection</typeparam>
        /// <param name="selection">The WeightedSelection itself</param>
        /// <returns>An IEnumerable of <typeparamref name="T"/></returns>
        public static IEnumerable<T> GetValues<T>(this WeightedSelection<T> selection)
        {
            return selection.choices.Select(x => x.value).Where(x => x != null);
        }

        /// <summary>
        /// Checks wether the specified unity object is valid. otherwise it returns null.
        /// <para>Allows for the usage of the .? and ?? operators with unity objects</para>
        /// </summary>
        public static T AsValidOrNull<T>(this T obj) where T : UnityEngine.Object
        {
            return obj ? obj : null;
        }

        /// <summary>
        /// Sets the value of a config entry without raising it's OnConfigChanged event
        /// </summary>
        /// <typeparam name="T">The type of value managed by the ConfigEntry</typeparam>
        /// <param name="entry">The ConfigEntry itself</param>
        /// <param name="newValue">The new value for the ConfigEntry</param>
        public static void SetValueWithoutNotifying<T>(this ConfigEntry<T> entry, T newValue)
        {
            _configEntryTypedValueField.SetValue(entry, newValue);
            entry.ConfigFile.Save();
        }

        /// <summary>
        /// Checks if the ItemDef is Available or not
        /// </summary>
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


        /// <summary>
        /// Checks if the EquipmentDef is Available or not
        /// </summary>
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

        /// <summary>
        /// Checks wether a Type is the same, or subclass of another type.
        /// </summary>
        public static bool IsSameOrSubclassOf(this Type type, Type otherType)
        {
            if (!(type == otherType))
            {
                return type.IsSubclassOf(otherType);
            }

            return true;
        }

        /// <summary>
        /// Checks if an InteractableGameObject can spawn extra objects.
        /// <para>This can be used to achieve a similar effect that interacting with interactables while holding Fireworks does.</para>
        /// </summary>
        /// <returns>True if the interactable can spawn extra objects, false otherwise</returns>
        public static bool IsInteractableValidForSpawns(GameObject interactableGameObject)
        {
            if (!interactableGameObject)
                return false;

            if (interactableGameObject.TryGetComponent<InteractionProcFilter>(out var val))
                return val.shouldAllowOnInteractionBeginProc;

            if (interactableGameObject.TryGetComponent<GenericPickupController>(out _))
                return false;

            if (interactableGameObject.TryGetComponent<VehicleSeat>(out _))
                return false;

            if (interactableGameObject.TryGetComponent<NetworkUIPromptController>(out _))
                return false;

            return true;
        }

        /// <summary>
        /// Extension method used for checking if a coroutine enumerator has finished executing.
        /// <para>The extension technically only calls <paramref name="coroutineEnumerator"/>'s MoveNext() method and negates the result. but this is mainly for syntactic sugar.</para>
        /// </summary>
        /// <param name="coroutineEnumerator">The coroutine enumerator to check</param>
        /// <returns>True if the coroutine has finished executing, false otherwise</returns>
        public static bool IsDone(this IEnumerator coroutineEnumerator)
        {
            return !coroutineEnumerator.MoveNext();
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