using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API.MiscHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Moonstorm.Loaders;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

namespace Moonstorm
{
    /// <summary>
    /// The ConfigurableFieldManager is a class that handles the usage of <see cref="ConfigurableFieldAttribute"/> in mods.
    /// </summary>
    public static class ConfigurableFieldManager
    {
        private static bool initialized = false;

        private static List<(List<FieldInfo>, string)> fieldsToConfigure = new List<(List<FieldInfo>, string)>();

        private static Dictionary<Assembly, string> assemblyToIdentifier = new Dictionary<Assembly, string>();
        private static Dictionary<string, List<FieldInfo>> identifierToFields = new Dictionary<string, List<FieldInfo>>();
        private static Dictionary<string, ConfigFile> identifierToConfigFile = new Dictionary<string, ConfigFile>();

        [SystemInitializer()]
        private static void Init()
        {
            initialized = true;

            MSULog.Info($"Initializing ConfigurableFieldManager");
            RoR2Application.onLoad += ConfigureFields;

            foreach(ConfigLoader loader in ConfigLoader.instances)
            {
                MSULog.Info($"Managing extra config files from {loader.OwnerMetaData.Name}'s ConfigLoader");
                foreach(var kvp in loader.identifierToConfigFile)
                {
                    try
                    {
                        if(identifierToConfigFile.ContainsKey(kvp.Key))
                        {
                            throw new InvalidOperationException($"Cannot add ConfigFile {kvp.Value} to the identifierToConfigFile because the identifier {kvp.Key} is already being used!");
                        }
                        identifierToConfigFile.Add(kvp.Key, kvp.Value);
                        MSULog.Debug($"Added config file {kvp.Value} with identifier {kvp.Key}");
                    }
                    catch (Exception ex) { MSULog.Error($"{ex} (Key: {kvp.Key}, Value: {kvp.Value})"); }
                }
            }
        }

        /// <summary>
        /// Adds the mod from <paramref name="baseUnityPlugin"/> into the ConfigurableFieldManager.
        /// <para>When added, the manager will look for Types with public static fields that implement the <see cref="ConfigurableFieldAttribute"/></para>
        /// <para>Mods added will also have the ability to use Configuration Files created with <see cref="ConfigLoader{T}"/> for the binding process</para>
        /// </summary>
        /// <param name="baseUnityPlugin">Your mod's BaseUnityPlugin inheriting class</param>
        public static void AddMod(BaseUnityPlugin baseUnityPlugin)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            var tuple = GetMainConfigFile(baseUnityPlugin);

            ConfigFile mainConfigFile = tuple.Item2;
            string mainConfigFileIdentifier = tuple.Item1;

            if (initialized)
            {
                MSULog.Warning($"Cannot add {assembly.GetName().Name} to the ConfigurableFieldManager as the manager has already been initialized.");
                return;
            }
            if (assemblyToIdentifier.ContainsKey(assembly))
            {
                MSULog.Warning($"Assembly {assembly.GetName().Name} has already been added to the ConfigurableFieldManager!");
                return;
            }
            if (mainConfigFile == null)
            {
                MSULog.Error($"Cannot add {assembly.GetName().Name} to the ConfigurableFieldManager as the assembly either does not have a type with the BepInPlugin attribute, or the type with the Attribute does not inherit from BaseUnityPlugin.");
                return;
            }

            MSULog.Info($"Adding mod {assembly.GetName().Name} to the configurable field manager");

            assemblyToIdentifier.Add(assembly, mainConfigFileIdentifier);

            if (!identifierToFields.ContainsKey(mainConfigFileIdentifier))
                identifierToConfigFile.Add(mainConfigFileIdentifier, mainConfigFile);

        }

        private static (string, ConfigFile) GetMainConfigFile(BaseUnityPlugin plugin)
        {
            plugin.Config.Save();
            return (plugin.Info.Metadata.GUID, plugin.Config);
        }

        private static void ConfigureFields()
        {
            var instances = SearchableAttribute.GetInstances<ConfigurableFieldAttribute>().OfType<ConfigurableFieldAttribute>();

            foreach(ConfigurableFieldAttribute configurableField in instances)
            {
                try
                {
                    FieldInfo field = (FieldInfo)configurableField.target;
                    Type declaringType = field.DeclaringType;

                    //Do not configure disabled content classes.
                    if (declaringType.GetCustomAttribute<DisabledContentAttribute>() != null)
                        continue;

                    string identifier = configurableField.ConfigFileIdentifier;
                    if(string.IsNullOrEmpty(identifier))
                    {
                        if(!assemblyToIdentifier.ContainsKey(declaringType.Assembly))
                        {
                            throw new KeyNotFoundException($"ConfigurableField for {declaringType.FullName}.{field.Name} does not have a ConfigFileIdentifier, and {declaringType.FullName}'s assembly is not in the ConfigurableFieldManager.");
                        }
                        identifier = assemblyToIdentifier[declaringType.Assembly];
                    }

                    if(!identifierToConfigFile.ContainsKey(identifier))
                    {
                        throw new KeyNotFoundException($"ConfigurableField for {declaringType.FullName}.{field.Name} has a ConfigFileIdentifier, but the identifier does not have a corresponding value.");
                    }

                    ConfigureField(configurableField, field, identifierToConfigFile[identifier]);
                }
                catch(Exception e)
                {
                    MSULog.Error($"Error while configuring {configurableField.target}\n{e}");
                }
            }
        }

        private static void ConfigureField(ConfigurableFieldAttribute attribute, FieldInfo field, ConfigFile config)
        {
            switch(field.GetValue(null))
            {
                case String _text: Bind<String>(field, config, _text, attribute); break;
                case Boolean _bool: Bind<Boolean>(field, config, _bool, attribute); break;
                case Byte _byte: Bind<Byte>(field, config, _byte, attribute); break;
                case SByte _sbyte: Bind<SByte>(field, config, _sbyte, attribute); break;
                case Int16 _int16: Bind<Int16>(field, config, _int16, attribute); break;
                case UInt16 _uint16: Bind<UInt16>(field, config, _uint16, attribute); break;
                case Int32 _int32: Bind<Int32>(field, config, _int32, attribute); break;
                case UInt32 _uint32: Bind<UInt32>(field, config, _uint32, attribute); break;
                case Int64 _int64: Bind<Int64>(field, config, _int64, attribute); break;
                case UInt64 _uint64: Bind<UInt64>(field, config, _uint64, attribute); break;
                case Single _single: Bind<Single>(field, config, _single, attribute); break;
                case Double _double: Bind<Double>(field, config, _double, attribute); break;
                case Decimal _decimal: Bind<Decimal>(field, config, _decimal, attribute); break;
                case Enum _enum: Bind<Enum>(field, config, _enum, attribute); break;
                case Color _color: Bind<Color>(field, config, _color, attribute); break;
                case Vector2 _vector2: Bind<Vector2>(field, config, _vector2, attribute); break;
                case Vector3 _vector3: Bind<Vector3>(field, config, _vector3, attribute); break;
                case Vector4 _vector4: Bind<Vector4>(field, config, _vector4, attribute); break;
                case Quaternion _quaternion: Bind<Quaternion>(field, config, _quaternion, attribute); break;
            }
        }

        private static void Bind<T>(FieldInfo field, ConfigFile config, T value, ConfigurableFieldAttribute attribute)
        {
            Type t = field.DeclaringType;
            field.SetValue(t, config.Bind<T>(attribute.GetSection(t), attribute.GetName(field), value, attribute.GetDescription()).Value);
        }
    }
}
