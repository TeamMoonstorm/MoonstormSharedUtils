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

        private static Dictionary<Assembly, string> assemblyToIdentifier = new Dictionary<Assembly, string>();
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
            identifierToConfigFile.Add(mainConfigFileIdentifier, mainConfigFile);
        }

        private static (string, ConfigFile) GetMainConfigFile(BaseUnityPlugin plugin)
        {
            plugin.Config.Save();
            return (plugin.Info.Metadata.GUID, plugin.Config);
        }

        private static void ConfigureFields()
        {
            var instances = SearchableAttribute.GetInstances<ConfigurableFieldAttribute>() ?? new List<SearchableAttribute>();
            MSULog.Info($"Configuring a total of {instances.Count()} fields");

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

                    ConfigureField(configurableField, identifierToConfigFile[identifier]);
                }
                catch(Exception e)
                {
                    MSULog.Error($"Error while configuring {configurableField.target}\n{e}");
                }
            }
        }

        private static void ConfigureField(ConfigurableFieldAttribute attribute, ConfigFile config)
        {
            switch(attribute.Field.GetValue(null))
            {
                case String _text: attribute.ConfigureField<string>(config, _text); break;
                case Boolean _bool: attribute.ConfigureField<bool>(config, _bool); break;
                case Byte _byte: attribute.ConfigureField<byte>(config, _byte); break;
                case SByte _sbyte: attribute.ConfigureField<sbyte>(config, _sbyte); break;
                case Int16 _int16: attribute.ConfigureField<short>(config, _int16); break;
                case UInt16 _uint16: attribute.ConfigureField<ushort>(config, _uint16); break;
                case Int32 _int32: attribute.ConfigureField<int>(config, _int32); break;
                case UInt32 _uint32: attribute.ConfigureField<uint>(config, _uint32); break;
                case Int64 _int64: attribute.ConfigureField<long>(config, _int64); break;
                case UInt64 _uint64: attribute.ConfigureField<ulong>(config, _uint64); break;
                case Single _single: attribute.ConfigureField<float>(config, _single); break;
                case Double _double: attribute.ConfigureField<double>(config, _double); break;
                case Decimal _decimal: attribute.ConfigureField<decimal>(config, _decimal); break;
                case Enum _enum: attribute.ConfigureField<Enum>(config, _enum); break;
                case Color _color: attribute.ConfigureField<Color>(config, _color); break;
                case Vector2 _vector2: attribute.ConfigureField<Vector2>(config, _vector2); break;
                case Vector3 _vector3: attribute.ConfigureField<Vector3>(config, _vector3); break;
                case Vector4 _vector4: attribute.ConfigureField<Vector4>(config, _vector4); break;
                case Quaternion _quaternion: attribute.ConfigureField<Quaternion>(config, _quaternion); break;
            }
        }
    }
}
