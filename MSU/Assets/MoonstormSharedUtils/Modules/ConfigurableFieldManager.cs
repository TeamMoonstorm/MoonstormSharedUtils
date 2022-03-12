using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API.MiscHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Moonstorm
{
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
        }

        public static void AddMod()
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            var tuple = GetMainConfigFile(assembly);

            ConfigFile mainConfigFile = tuple.Item2;
            string mainConfigFileIdentifier = tuple.Item1;

            if(initialized)
            {
                MSULog.Warning($"Cannot add {assembly.GetName().Name} to the ConfigurableFieldManager as the manager has already been initialized.");
                return;
            }
            if(assemblyToIdentifier.ContainsKey(assembly))
            {
                MSULog.Warning($"Assembly {assembly.GetName().Name} has already been added to the ConfigurableFieldManager!");
                return;
            }
            if(mainConfigFile == null)
            {
                MSULog.Error($"Cannot add {assembly.GetName().Name} to the ConfigurableFieldManager as the assembly either does not have a type with the BepInPlugin attribute, or the type with the Attribute does not inherit from BaseUnityPlugin.");
                return;
            }

            MSULog.Info($"Adding mod {assembly.GetName().Name} to the configurable field manager");

            assemblyToIdentifier.Add(assembly, mainConfigFileIdentifier);

            if (!identifierToFields.ContainsKey(mainConfigFileIdentifier))
                identifierToConfigFile.Add(mainConfigFileIdentifier, mainConfigFile);

            Dictionary<string, List<FieldInfo>> dict = new Dictionary<string, List<FieldInfo>>();

            foreach(Type type in assembly.GetTypes().Where(type => type.GetCustomAttribute<DisabledContentAttribute>() == null))
            {
                try
                {
                    var validFields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                                          .Where(field => field.GetCustomAttribute<ConfigurableFieldAttribute>() != null)
                                          .ToList();

                    if(validFields.Count > 0)
                    {
                        foreach(FieldInfo field in validFields)
                        {
                            try
                            {
                                var attribute = field.GetCustomAttribute<ConfigurableFieldAttribute>();
                                string configIdeentifier = attribute.configFileIdentifier ?? assemblyToIdentifier[field.DeclaringType.Assembly]; //If configFilePath is null, use the main config file's path, otherwise, use the provided one.
                                if(!dict.ContainsKey(configIdeentifier))
                                {
                                    dict.Add(configIdeentifier, new List<FieldInfo>());
                                }
                                dict[configIdeentifier].Add(field);
                            }
                            catch (Exception e) { MSULog.Error(e); }
                        }
                    }
                }
                catch(Exception e) { MSULog.Error(e); }
            }

            if(dict.Count == 0)
            {
                MSULog.Warning($"Found no fields that have the {nameof(ConfigurableFieldAttribute)} attribute within {assembly.GetName().Name}");
                return;
            }

            MSULog.Debug($"Found a total of {dict.Values.SelectMany(x => x).Count()} fields with the {nameof(ConfigurableFieldAttribute)}");

            foreach(var (identifier, fields) in dict)
            {
                if(identifierToFields.ContainsKey(identifier))
                {
                    MSULog.Warning($"ConfigFilePathToFields already has a key with name {identifier}! is this intentional?");
                    identifierToFields[identifier].AddRange(fields);
                    continue;
                }
                identifierToFields.Add(identifier, fields);
            }
        }

        private static void AddConfigFile(ConfigFile configFile, string uniqueIdentifier)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            if(configFile == GetMainConfigFile(assembly).Item2)
            {
                MSULog.Error($"Cannot add config file {configFile} because its the main config file of {assembly}!" +
                    $"The identifier of the main config file is the Mod's GUID");
                return;
            }
            if(identifierToConfigFile.ContainsKey(uniqueIdentifier))
            {
                MSULog.Error($"Cannot add config file {configFile} because its already in the dictionary!");
                return;
            }

            identifierToConfigFile.Add(uniqueIdentifier, configFile);
        }

        private static (string, ConfigFile) GetMainConfigFile(Assembly assembly)
        {
            Type bepInPluginType = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<BepInPlugin>() != null)
                .FirstOrDefault();

            if(bepInPluginType == null)
            {
                MSULog.Error($"Could not find main class of assembly {assembly}! cannot retrieve Config Tuple.");
                return (null, null);
            }
            object typeAsObj = bepInPluginType;
            if (!(typeAsObj is BaseUnityPlugin))
            {
                MSULog.Error($"The type {bepInPluginType} does not inherit from BaseUnityPlugin! cannot retrieve Config Tuple.");
                return (null, null);
            }
            var baseUP = (BaseUnityPlugin)typeAsObj;
            return (bepInPluginType.GetCustomAttribute<BepInPlugin>().GUID, baseUP.Config);
        }

        private static void ConfigureFields()
        {
            List<FieldInfo> count = identifierToFields.Values.SelectMany(x => x).ToList();

            MSULog.Info($"Configuring a total of {count.Count} Fields.");

            foreach(var (identifier, fields) in identifierToFields)
            {
                try
                {
                    if (!identifierToConfigFile.ContainsKey(identifier))
                        throw new NullReferenceException($"Could not find a matching config file with the identifier {identifier}!");
                    
                    foreach(var field in fields)
                    {
                        try
                        {
                            ConfigureField(field, identifierToConfigFile[identifier]);
                        }
                        catch(Exception e) { MSULog.Error(e); }
                    }
                }
                catch(Exception e) { MSULog.Error(e); }
            }
        }

        private static void ConfigureField(FieldInfo field, ConfigFile config)
        {
            MSULog.Debug($"Configuring {field.Name}");

            var attribute = field.GetCustomAttribute<ConfigurableFieldAttribute>(true);

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
