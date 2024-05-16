using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU.Config
{
    /// <summary>
    /// Handles the Configuration system of MSU, this includes the proper implementation of <see cref="ConfigureFieldAttribute"/>, <see cref="RiskOfOptionsConfigureFieldAttribute"/>, <see cref="ConfiguredVariable.AutoConfigAttribute"/> and classes dervied from <see cref="ConfiguredVariable{T}"/>
    /// </summary>
    public static class ConfigSystem
    {
        private static readonly Dictionary<ConfigFile, BaseUnityPlugin> _configToPluginOwner = new Dictionary<ConfigFile, BaseUnityPlugin>();
        private static readonly Dictionary<string, ConfigFile> _identifierToConfigFile = new Dictionary<string, ConfigFile>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<ConfigFile> _configFilesWithSeparateRiskOfOptionsEntries = new HashSet<ConfigFile>();
        
        /// <summary>
        /// Retrieves a <see cref="ConfigFile"/> with the identifier specified in <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">The identifier of the ConfigFile</param>
        /// <returns>A valid ConfigFile if it exists in the ConfigSystem, otherwise returns null.</returns>
        public static ConfigFile GetConfigFile(string identifier)
        {
            if(!_identifierToConfigFile.TryGetValue(identifier, out ConfigFile configFile))
            {
#if DEBUG
                MSULog.Warning($"Couldd not find a config file with the identifier \"{identifier}\"");
#endif
                return null;
            }
            return configFile;
        }

        /// <summary>
        /// Adds a <see cref="ConfigFile"/> with a corresponding identifier.
        /// <para>You're strongly advised to create new ConfigFiles by using <see cref="ConfigFactory.CreateConfigFile(string, bool)"/>, as that method automatically calls this one.</para>
        /// </summary>
        /// <param name="identifier">The identifier for <paramref name="configFile"/></param>
        /// <param name="configFile">The ConfigFile which will be identified using <paramref name="identifier"/></param>
        /// <param name="tiedPlugin">The plugint that's respnsible for adding <paramref name="configFile"/></param>
        /// <param name="createSeparateRiskOfOptionsEntry">If true, the ConfigSystem will create a new Risk of Options entry for the ConfigFile.</param>
        public static void AddConfigFileAndIdentifier(string identifier, ConfigFile configFile, BaseUnityPlugin tiedPlugin, bool createSeparateRiskOfOptionsEntry = false)
        { 
            if(_identifierToConfigFile.ContainsKey(identifier))
            {
                MSULog.Warning($"Cannot add a ConfigFile with the identifier \"{identifier}\" as that identifier is already being used.");
                return;
            }

            _configToPluginOwner.Add(configFile, tiedPlugin);
            _identifierToConfigFile.Add(identifier, configFile);
            if(createSeparateRiskOfOptionsEntry)
                _configFilesWithSeparateRiskOfOptionsEntries.Add(configFile);
        }

        /// <summary>
        /// Checks wether the specified ConfigFile in <paramref name="cf"/> should have a separate RiskOfOptions entry.
        /// </summary>
        /// <param name="cf">The config file to check</param>
        /// <returns>True if risk of options should create a separate entry, false otherwise.</returns>
        public static bool ShouldCreateSeparateRiskOfOptionsEntry(ConfigFile cf) => _configFilesWithSeparateRiskOfOptionsEntries.Contains(cf);

        private static PluginInfo FindPluginInfo(Assembly asm)
        {
            foreach(var (name, pInfo) in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (asm.Location == pInfo.Location)
                    return pInfo;
            }
            return null;
        }

        [SystemInitializer]
        private static void Init()
        {
            RoR2Application.onLoad += BindConfig;
        }

        private static void BindConfig()
        {
            BindConfigureFieldAttributes();
            BindAutoConfigAttributes();
        }

        private static void BindConfigureFieldAttributes()
        {
            List<ConfigureFieldAttribute> instances = new List<ConfigureFieldAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances(instances);
            if (instances.Count <= 0)
                return;

            MSULog.Info($"Configuring a total of {instances.Count} ConfigureField attributes");
            foreach(ConfigureFieldAttribute attribute in instances)
            {
                try
                {
                    ConfigureConfigureField(attribute);
                }
                catch(Exception e)
                {
                    MSULog.Error($"Error while configuring {attribute.AttachedMemberInfo.Name}. {e}");
                }
            }
        }

        private static void ConfigureConfigureField(ConfigureFieldAttribute attribute)
        {
            MemberInfo memberInfo = attribute.AttachedMemberInfo;
            Type declaringType = memberInfo.DeclaringType;
            Func<object> getAction = null;
            switch(memberInfo)
            {
                case PropertyInfo pInfo:
                    var methodInfo = pInfo.GetGetMethod();
                    getAction = () => methodInfo?.Invoke(null, null);
                    methodInfo = pInfo.GetSetMethod();
                    break;
                case FieldInfo fieldInfo:
                    getAction = () => fieldInfo.GetValue(null);
                    break;
                default:
                    throw new InvalidOperationException("ConfiguredVariable.AutoConfig is only valid on Property and Field members.");
            }

            if (getAction == null)
                return;

            var val = getAction();
            Type valueType = val.GetType();
            if (!TomlTypeConverter.CanConvert(valueType))
            {
                throw new InvalidOperationException($"ConfigureField attribute for {declaringType.FullName}.{memberInfo.Name} cannot be configured as the Field's Type ({valueType.FullName}) is not supported by BepInEx's TomlTypeConverter");
            }

            string identifier = attribute.ConfigFileIdentifier;
            PluginInfo pluginInfo = FindPluginInfo(declaringType.Assembly);
            ConfigFile file = identifier.IsNullOrWhiteSpace() ? pluginInfo.Instance.Config : GetConfigFile(identifier);

            if(attribute is RiskOfOptionsConfigureFieldAttribute rooAttribute)
            {
                rooAttribute.ModGUID = pluginInfo.Metadata.GUID;
                rooAttribute.ModName = pluginInfo.Metadata.Name;
            }

            ConfigureInternal(attribute, file, val);

            void ConfigureInternal(ConfigureFieldAttribute att, ConfigFile configFile, object value)
            {
                switch(val)
                {
                    case string _string: att.ConfigureField(configFile, _string); break;
                    case bool _bool: att.ConfigureField(configFile, _bool); break;
                    case byte _byte: att.ConfigureField(configFile, _byte); break;
                    case sbyte _sbyte: att.ConfigureField(configFile, _sbyte); break;
                    case short _short: att.ConfigureField(configFile, _short); break;
                    case ushort _ushort: att.ConfigureField(configFile, _ushort); break;
                    case int _int: att.ConfigureField(configFile, _int); break;
                    case uint _uint: att.ConfigureField(configFile, _uint); break;
                    case long _long: att.ConfigureField(configFile, _long); break;
                    case ulong _ulong: att.ConfigureField(configFile, _ulong); break;
                    case float _float: att.ConfigureField(configFile, _float); break;
                    case double _double: att.ConfigureField(configFile, _double); break;
                    case decimal _decimal: att.ConfigureField(configFile, _decimal); break;
                    case Enum _enum: att.ConfigureField(configFile, _enum); break;
                    case Color _color: att.ConfigureField<Color>(configFile, _color); break;
                    case Vector2 _vector2: att.ConfigureField<Vector2>(configFile, _vector2); break;
                    case Vector3 _vector3: att.ConfigureField<Vector3>(configFile, _vector3); break;
                    case Vector4 _vector4: att.ConfigureField<Vector4>(configFile, _vector4); break;
                    case Quaternion _quaternion: att.ConfigureField<Quaternion>(configFile, _quaternion); break;
                    case KeyboardShortcut _keyboardShortcut: att.ConfigureField<KeyboardShortcut>(configFile, _keyboardShortcut); break;
                    default: MSULog.Error($"Due to the nature of the Config system in bepinex, MSU cannot bind a config of type {val.GetType().FullName}. If you see this, please create an issue on our github."); break;
                }
            }
        }

        private static void BindAutoConfigAttributes()
        {
            List<ConfiguredVariable.AutoConfigAttribute> instances = new List<ConfiguredVariable.AutoConfigAttribute>();
            HG.Reflection.SearchableAttribute.GetInstances(instances);
            if (instances.Count <= 0)
                return;

            MSULog.Info($"Auto Configuring a total of {instances.Count} ConfiguredVariables");

            foreach(ConfiguredVariable.AutoConfigAttribute instance in instances)
            {
                try
                {
                    ConfigureAutoConfig(instance);
                }
                catch(Exception e)
                {
                    MSULog.Error($"Error while auto configuring a ConfiguredVariable: {instance}\n{e}");
                }
            }
        }

        private static void ConfigureAutoConfig(ConfiguredVariable.AutoConfigAttribute instance)
        {
            MemberInfo memberInfo = (MemberInfo)instance.target;
            ConfiguredVariable configuredVariable = null;
            switch(memberInfo)
            {
                case PropertyInfo pInfo:
                    var methodInfo = pInfo.GetGetMethod();
                    configuredVariable = (ConfiguredVariable)methodInfo?.Invoke(null, null);
                    break;
                case FieldInfo fInfo:
                    configuredVariable = (ConfiguredVariable)fInfo.GetValue(null);
                    break;
                default:
                    throw new InvalidOperationException("ConfiguredVariable.AutoConfig is only valid on Property and Field members.");
            }

            if (configuredVariable == null)
                return;

            if (configuredVariable.IsConfigured)
                return;

            if (!configuredVariable.CanBeConfigured)
                return;

            if(configuredVariable.ConfigFile == null)
            {
                ConfigFile file = configuredVariable.ConfigFileIdentifier.IsNullOrWhiteSpace() ? FindPluginInfo(memberInfo.DeclaringType.Assembly).Instance.Config : GetConfigFile(configuredVariable.ConfigFileIdentifier);

                if (file == null)
                    throw new NullReferenceException("Configfile is null");

                configuredVariable.ConfigFile = file;
            }

            configuredVariable.Section = configuredVariable.Section.IsNullOrWhiteSpace() ? MSUtil.NicifyString(memberInfo.DeclaringType.Name) : configuredVariable.Section;
            configuredVariable.Key = configuredVariable.Key.IsNullOrWhiteSpace() ? MSUtil.NicifyString(memberInfo.Name) : configuredVariable.Key;
            if (configuredVariable.Description.IsNullOrWhiteSpace() || !instance.DescriptionOverride.IsNullOrWhiteSpace())
            {
                configuredVariable.Description = instance.DescriptionOverride;
            }

            if(configuredVariable.ModGUID.IsNullOrWhiteSpace())
            {
                configuredVariable.ModGUID = _configToPluginOwner[configuredVariable.ConfigFile].Info.Metadata.GUID;
            }
            
            if(configuredVariable.ModName.IsNullOrWhiteSpace())
            {
                configuredVariable.ModName = _configToPluginOwner[configuredVariable.ConfigFile].Info.Metadata.Name;
            }
            configuredVariable.Configure();
        }
    }
}
