using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Config;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moonstorm.Loaders;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="ConfigSystem"/> is a class that handles the ConfigurationSystems of MSU, this includes the <see cref="ConfigurableFieldAttribute"/> and classes deriving from <see cref="ConfigurableVariable"/>
    /// </summary>
    public static class ConfigSystem
    {
        private struct ManagedModData
        {
            public PluginInfo pluginInfo;
            public ConfigFile mainConfigFile;
            public Assembly tiedAssembly;
            public (MemberInfo, ConfigurableVariable)[] ConfigurableVariables;

            public ManagedModData(BaseUnityPlugin plugin, (MemberInfo, ConfigurableVariable)[] configurableVariables)
            {
                pluginInfo = plugin.Info;
                mainConfigFile = plugin.Config;
                tiedAssembly = plugin.GetType().Assembly;
                ConfigurableVariables = configurableVariables;
            }
        }

        private static Dictionary<string, ManagedModData> assemblyNameToModData = new Dictionary<string, ManagedModData>();
        private static Dictionary<string, ConfigFile> identifierToConfigFile = new Dictionary<string, ConfigFile>();

        private static bool initialized = false;

        /// <summary>
        /// Adds the mod from <paramref name="baseUnityPlugin"/> into the ConfigSystem.
        /// <para>When added, the System will automatically implement the configuration of any <see cref="ConfigurableFieldAttribute"/>s and <see cref="ConfigurableVariable"/>s in your mod</para>
        /// </summary>
        /// <param name="baseUnityPlugin">Your mod's BaseUnityPlugin inheriting class.</param>
        public static void AddMod(BaseUnityPlugin baseUnityPlugin)
        {
            if (initialized)
                return;

            Assembly assembly = baseUnityPlugin.GetType().Assembly;
            if (assemblyNameToModData.ContainsKey(assembly.FullName))
                return;

            ManagedModData modData = new ManagedModData(baseUnityPlugin, GetConfigurableVariables(assembly));
            assemblyNameToModData.Add(assembly.FullName, modData);
            AddConfigFileAndIdentifier(modData.pluginInfo.Metadata.GUID, modData.mainConfigFile);
        }

        /// <summary>
        /// Retrieves a <see cref="ConfigFile"/> with the identifier specified in <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">The identifier of the ConfigFile</param>
        /// <returns>A valid ConfigFile if it exists in the ConfigSystem, null otherwise.</returns>
        public static ConfigFile GetConfigFile(string identifier)
        {
            if (identifierToConfigFile.TryGetValue(identifier, out ConfigFile configFile))
            {
                return configFile;
            }
            return null;
        }

        /// <summary>
        /// Adds a <see cref="ConfigFile"/> with it's corresponding Identifier.
        /// <para>You're strongly advised to create new ConfigFiles by using the <see cref="ConfigLoader"/> class, as that class automatically calls this method.</para>
        /// </summary>
        /// <param name="identifier">The identifier for <paramref name="configFile"/></param>
        /// <param name="configFile">The ConfigFile which will be identified using <paramref name="identifier"/></param>
        public static void AddConfigFileAndIdentifier(string identifier, ConfigFile configFile)
        {
            if (initialized)
                return;

            if (identifierToConfigFile.ContainsKey(identifier))
                return;

            identifierToConfigFile.Add(identifier, configFile);
        }
        [SystemInitializer]
        private static void Init()
        {
            initialized = true;
            MSULog.Info("Initializing ConfigSystem");

            RoR2Application.onLoad += BindConfigs;
        }

        private static void BindConfigs()
        {
            BindConfigurableFieldAttributes();
            BindConfigurableVariables();
        }

        private static void BindConfigurableFieldAttributes()
        {
            var instances = HG.Reflection.SearchableAttribute.GetInstances<ConfigurableFieldAttribute>() ?? new List<HG.Reflection.SearchableAttribute>();
            MSULog.Info($"Configuring a total of {instances.Count} fields");
            foreach (ConfigurableFieldAttribute configurableField in instances)
            {
                try
                {
                    FieldInfo field = configurableField.Field;
                    Type declaringType = field.DeclaringType;

                    if (!TomlTypeConverter.CanConvert(field.FieldType))
                    {
                        throw new InvalidOperationException($"ConfigurableField for {declaringType.FullName}.{field.Name} cannot be configured as the Field's Type ({field.FieldType.FullName}) is not supported by BepInEx's TomlTypeConverter");
                    }

                    if (declaringType.GetCustomAttribute<DisabledContentAttribute>() != null)
                        continue;

                    string asmName = declaringType.Assembly.FullName;
                    if (!assemblyNameToModData.ContainsKey(asmName))
                    {
                        throw new InvalidOperationException($"ConfigurableField for {declaringType.FullName}.{field.Name} cannot be configured as it's assembly is not in the ConfigSystem");
                    }

                    string identifier = configurableField.ConfigFileIdentifier;
                    ConfigFile file = identifier.IsNullOrWhiteSpace() ? assemblyNameToModData[asmName].mainConfigFile : GetConfigFile(configurableField.ConfigFileIdentifier);

                    if (configurableField is RooConfigurableFieldAttribute att)
                    {
                        PluginInfo pInfo = assemblyNameToModData[asmName].pluginInfo;
                        att.OwnerGUID = pInfo.Metadata.GUID;
                        att.OwnerName = pInfo.Metadata.Name;
                    }

                    ConfigureField(configurableField, file);
                }
                catch (Exception e)
                {
                    MSULog.Error($"Error while configuring {configurableField.Field.DeclaringType.Name}.{configurableField.Field.Name}\n{e}");
                }
            }

            void ConfigureField(ConfigurableFieldAttribute attribute, ConfigFile config)
            {
                switch (attribute.Field.GetValue(null))
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
                    default: MSULog.Error($"Due to the nature of the Config system in bepinex, MSU cannot bind a config of type {attribute.Field.FieldType}. If you see this, please create an issue on our github."); break;
                }
            }
        }

        private static void BindConfigurableVariables()
        {
            foreach (ManagedModData data in assemblyNameToModData.Values)
            {
                BindConfigurableVariablesForMod(data);
            }
        }

        private static void BindConfigurableVariablesForMod(ManagedModData data)
        {

            foreach (var (memberInfo, configurableVariable) in data.ConfigurableVariables)
            {
                try
                {
                    if (configurableVariable.IsConfigured || !configurableVariable.IsActuallyConfigurable)
                        continue;

                    if (configurableVariable.ConfigFile == null)
                    {
                        configurableVariable.ConfigFile = configurableVariable.ConfigIdentifier.IsNullOrWhiteSpace() ? data.mainConfigFile : GetConfigFile(configurableVariable.ConfigIdentifier);
                    }

                    configurableVariable.Section = configurableVariable.Section.IsNullOrWhiteSpace() ? MSUtil.NicifyString(memberInfo.DeclaringType.Name) : configurableVariable.Section;
                    configurableVariable.Key = configurableVariable.Key.IsNullOrWhiteSpace() ? MSUtil.NicifyString(memberInfo.Name) : configurableVariable.Key;
                    configurableVariable.Description = configurableVariable.Description.IsNullOrWhiteSpace() ? "Configure this variable" : configurableVariable.Description;

                    configurableVariable.ModGUID = configurableVariable.ModGUID.IsNullOrWhiteSpace() ? data.pluginInfo.Metadata.GUID : configurableVariable.ModGUID;
                    configurableVariable.ModName = configurableVariable.ModName.IsNullOrWhiteSpace() ? data.pluginInfo.Metadata.Name : configurableVariable.ModName;

                    configurableVariable.Configure();
                }
                catch (Exception ex)
                {
                    MSULog.Error(ex);
                }
            }
        }
        private static (MemberInfo, ConfigurableVariable)[] GetConfigurableVariables(Assembly assembly)
        {
            Type[] types = assembly.GetTypesSafe();
            MemberInfo[] memberInfos = types.SelectMany(t => t.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToArray();

            FieldInfo[] fields = memberInfos.OfType<FieldInfo>().ToArray();
            PropertyInfo[] properties = memberInfos.OfType<PropertyInfo>().ToArray();

            ConfigurableVariable cVar = null;
            List<(MemberInfo, ConfigurableVariable)> cVars = new List<(MemberInfo, ConfigurableVariable)>();
            foreach (FieldInfo field in fields)
            {
                if (!field.FieldType.IsSubclassOf(typeof(ConfigurableVariable)))
                {
                    continue;
                }
                cVar = (ConfigurableVariable)field.GetValue(null);
                cVars.Add((field, cVar));
            }

            foreach (PropertyInfo property in properties)
            {
                if (!property.PropertyType.IsSubclassOf(typeof(ConfigurableVariable)))
                {
                    continue;
                }
                var methodInfo = property.GetGetMethod();
                if (methodInfo == null)
                {
                    continue;
                }
                cVar = (ConfigurableVariable)methodInfo.Invoke(null, null);
                cVars.Add((property, cVar));
            }

            return cVars.ToArray();
        }
    }
}
