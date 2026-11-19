using BepInEx.Configuration;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

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
        public static bool debugToolkitInstalled => IsModInstalled("iHarbHD.DebugToolkit");
        /// <summary>
        /// Wether or not HolyDLL by RandomlyAwesome is installed
        /// </summary>
        public static bool holyDLLInstalled => IsModInstalled("xyz.yekoc.Holy");
        private static Run _currentRun;
        private static ExpansionDef[] _currentRunExpansionDefs = Array.Empty<ExpansionDef>();
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
        /// <summary>
        /// DEBUG ONLY
        /// <para></para>
        /// Invokes a command of name <paramref name="commandName"/> with the arguments specified in <paramref name="arguments"/>
        /// </summary>
        /// <param name="commandName">The name of the command</param>
        /// <param name="arguments">The arguments for the command</param>
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
        /// <returns>An array of the enabled <see cref="ExpansionDef"/>s, or an empty array if the run instance is null.</returns>
        public static ExpansionDef[] GetEnabledExpansions(this Run run)
        {
            if (!run)
            {
                return Array.Empty<ExpansionDef>();
            }
            if (_currentRun == run)
            {
                return _currentRunExpansionDefs;
            }

            _currentRun = run;
            _currentRunExpansionDefs = ExpansionCatalog.expansionDefs.Where(x => run.IsExpansionEnabled(x)).ToArray();
            return _currentRunExpansionDefs;
        }

        [Obsolete("Use HG.UnityObjectExtensions.EnsureComponent<T>(this GameObject) instead.", error: true)]
        public static T EnsureComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            return HG.UnityObjectExtensions.EnsureComponent<T>(obj);
        }


        [Obsolete("Use HasItem(CharacterBody, ItemDef, ItemStorageType storageType) instead", error: true)]
        public static bool HasItem(this CharacterBody body, ItemDef def) => HasItem(body, def, ItemStorageType.Effective);

        /// <summary>
        /// Returns true if <paramref name="body"/> has at least one stack of <paramref name="itemDef"/>. By default it checks the Effective Item Stacks, but this can be changed by setting <paramref name="storageType"/> to a value that's not <see cref="ItemStorageType.Count"/> or <see cref="ItemStorageType.None"/>
        /// </summary>
        public static bool HasItem(this CharacterBody body, ItemDef itemDef, ItemStorageType storageType = ItemStorageType.Effective) => HasItem(body, itemDef ? itemDef.itemIndex : ItemIndex.None, storageType);

        [Obsolete("Use HasItem(CharacterBody, ItemIndex, ItemStorageType) instead", error: true)]
        public static bool HasItem(this CharacterBody body, ItemIndex index) => HasItem(body, index, ItemStorageType.Effective);

        /// <summary>
        /// Returns true if <paramref name="body"/> has at least one stack of <paramref name="itemIndex"/>. By default it checks the Effective Item Stacks, but this can be changed by setting <paramref name="storageType"/> to a value that's not <see cref="ItemStorageType.Count"/> or <see cref="ItemStorageType.None"/>
        /// </summary>
        public static bool HasItem(this CharacterBody body, ItemIndex itemIndex, ItemStorageType storageType = ItemStorageType.Effective) => GetItemCount(body, itemIndex, storageType) > 0;

        [Obsolete("Use TryGetItemCount(CharacterBody, ItemDef, out int, ItemStorageType) instead", error: true)]
        public static bool TryGetItemCount(this CharacterBody body, ItemDef itemDef, out int itemCount) => TryGetItemCount(body, itemDef, out itemCount, ItemStorageType.Effective);

        /// <summary>
        /// Method used to attempt to retrieve the Item Count of <paramref name="itemDef"/> in the inventory of <paramref name="body"/>. The count is outputted on <paramref name="itemCount"/>. By default it checks the Effective Item Stacks, but this can be changed by setting <paramref name="storageType"/> to a value that's not <see cref="ItemStorageType.Count"/> or <see cref="ItemStorageType.None"/>.
        /// <para></para>
        /// This method returns false if the body has no inventory, if <paramref name="storageType"/> is an invalid value, or if the body has no item stacks for <paramref name="index"/>
        /// </summary>
        public static bool TryGetItemCount(this CharacterBody body, ItemDef itemDef, out int itemCount, ItemStorageType storageType = ItemStorageType.Effective) => TryGetItemCount(body, itemDef ? itemDef.itemIndex : ItemIndex.None, out itemCount, storageType);

        [Obsolete("Use TryGetItemCount(CharacterBody, ItemIndex, out int, ItemStorageType) instead", error: true)]
        public static bool TryGetItemCount(this CharacterBody body, ItemIndex index, out int itemCount) => TryGetItemCount(body, index, out itemCount, ItemStorageType.Effective);

        /// <summary>
        /// Method used to attempt to retrieve the Item Count of <paramref name="index"/> in the inventory of <paramref name="body"/>. The count is outputted on <paramref name="itemCount"/>. By default it checks the Effective Item Stacks, but this can be changed by setting <paramref name="storageType"/> to a value that's not <see cref="ItemStorageType.Count"/> or <see cref="ItemStorageType.None"/>.
        /// <para></para>
        /// This method returns false if the body has no inventory, if <paramref name="storageType"/> is an invalid value, or if the body has no item stacks for <paramref name="index"/>
        /// </summary>
        public static bool TryGetItemCount(this CharacterBody body, ItemIndex index, out int itemCount, ItemStorageType storageType = ItemStorageType.Effective)
        {
            itemCount = 0;
            if(body && body.inventory)
            {
                Inventory inventory = body.inventory;

                switch(storageType)
                {
                    case ItemStorageType.None:
                    case ItemStorageType.Count:
#if DEBUG
                        MSULog.Warning($"Invalid ItemStorageType used in TryGetItemCount(). StorageType used: {storageType}");
#endif
                        itemCount = 0;
                        return false;
                    default:
                        itemCount = inventory.GetStorageItemCount(index, storageType);
                        return itemCount > 0;
                }
            }
            return false;
        }

        [Obsolete("Use GetItemCount(CharacterBody, ItemDef, ItemStorageType) instead", error: true)]
        public static int GetItemCount(this CharacterBody body, ItemDef itemDef) => GetItemCount(body, itemDef, ItemStorageType.Effective);

        /// <summary>
        /// Retrieves the item count of <paramref name="itemDef"/> in the inventory of <paramref name="body"/>. By default it checks the Effective Item Stacks, but this can be changed by setting <paramref name="storageType"/> to a value that's not <see cref="ItemStorageType.Count"/> or <see cref="ItemStorageType.None"/>.
        /// <para></para>
        /// If the body's inventory doesnt have a stack of <paramref name="itemDef"/>, the value returned is 0.
        /// </summary>
        public static int GetItemCount(this CharacterBody body, ItemDef itemDef, ItemStorageType storageType = ItemStorageType.Effective) => GetItemCount(body, itemDef ? itemDef.itemIndex : ItemIndex.None, storageType);

        [Obsolete("Use GetItemCount(CharacterBody, ItemIndex, ItemStorageType) instead", error: true)]
        public static int GetItemCount(this CharacterBody body, ItemIndex index) => GetItemCount(body, index, ItemStorageType.Effective);

        /// <summary>
        /// Retrieves the item count of <paramref name="index"/> in the inventory of <paramref name="body"/>. By default it checks the Effective Item Stacks, but this can be changed by setting <paramref name="storageType"/> to a value that's not <see cref="ItemStorageType.Count"/> or <see cref="ItemStorageType.None"/>.
        /// <para></para>
        /// If the body's inventory doesnt have a stack of <paramref name="index"/>, the value returned is 0.
        /// </summary>
        public static int GetItemCount(this CharacterBody body, ItemIndex index, ItemStorageType storageType = ItemStorageType.Effective)
        {
            if(body.TryGetItemCount(index, out int count, storageType))
            {
                return count;
            }
            return 0;
        }

        /// <summary>
        /// Deconstructs a KeyValuePair into two outgoing variables, useful for iterating dictionaries.
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        [Obsolete("Use \"Util.TryNicifyString(string)\" instead", error: true)]
        public static string NicifyString(string text)
        {
            return Util.TryNicifyString(text);
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

        [Obsolete("Use HG.UnityObjectExtensions.AsValidOrNull(this T) instead")]
        public static T AsValidOrNull<T>(this T obj) where T : UnityEngine.Object
        {
            return HG.UnityObjectExtensions.AsValidOrNull(obj);
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

        [Obsolete("Call \"GlobalEventManager.InteractableIsPermittedForSpawn(MonoBehaviour, InteractionProcFilter)\" instead")]
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

        [Obsolete("This method can easily conflict with PascalCase properties. Replace this call with a \"MoveNext()\" and remove the negation", error: true)]
        public static bool IsDone(this IEnumerator coroutineEnumerator)
        {
            return !coroutineEnumerator.MoveNext();
        }

        /// <summary>
        /// Write <paramref name="gameplayEventIndex"/> as an Int32 to the network writer
        /// </summary>
        /// <param name="writer">The writer writing</param>
        /// <param name="gameplayEventIndex">The index to write</param>
        public static void Write(this NetworkWriter writer, GameplayEventIndex gameplayEventIndex)
        {
            writer.WritePackedIndex32((int)gameplayEventIndex);
        }

        /// <summary>
        /// Read the incoming data of <paramref name="reader"/> as a <see cref="GameplayEventIndex"/>
        /// </summary>
        /// <param name="reader">The reader reading</param>
        /// <returns>The GameplayEventIndex</returns>
        public static GameplayEventIndex ReadGameplayEventIndex(this NetworkReader reader)
        {
            return (GameplayEventIndex)reader.ReadPackedIndex32();
        }

        /// <summary>
        /// Write <paramref name="genericObjectIndex"/> as an Int32 to the network writer
        /// </summary>
        /// <param name="writer">The writer writing</param>
        /// <param name="genericObjectIndex">The index to write</param>
        public static void Write(this NetworkWriter writer, GenericObjectIndex genericObjectIndex)
        {
            writer.WritePackedIndex32((int)genericObjectIndex);
        }


        /// <summary>
        /// Read the incoming data of <paramref name="reader"/> as a <see cref="GenericObjectIndex"/>
        /// </summary>
        /// <param name="reader">The reader reading</param>
        /// <returns>The GameplayEventIndex</returns>
        public static GenericObjectIndex ReadGenericObjectIndex(this NetworkReader reader)
        {
            return (GenericObjectIndex)reader.ReadPackedIndex32();
        }

        /// <summary>
        /// Returns true if the Collection is null or Empty (Count == 0)
        /// </summary>
        public static bool IsCollectionEmptyOrNull(ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }

        [Obsolete(NullableRef<object>.OBSOLETE_MESSAGE)]
        internal static T GetValueFromNullableRefOrRegularField<T>(NullableRef<T> nullableRefValue) where T : class
        {
            if(nullableRefValue.hasValue)
            {
                return nullableRefValue.value;
            }
            return null;
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