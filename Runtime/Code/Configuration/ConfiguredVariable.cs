using BepInEx;
using BepInEx.Configuration;
using HG.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSU.Config
{
    /// <summary>
    /// A NonGeneric version of a <see cref="ConfiguredVariable{T}"/>, You're strongly advised to use <see cref="ConfiguredVariable{T}"/> instead.
    /// </summary>
    public abstract class ConfiguredVariable
    {
        /// <summary>
        /// The <see cref="ConfigFile"/> that this ConfiguredVariable is bound to.
        /// <para>If left null before the Configuration process, a CopnfigFile is attempted to be obtained using <see cref="ConfigSystem.GetConfigFile(string)"/> and the value stored in <see cref="ConfigFileIdentifier"/></para>
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true.</para>
        /// </summary>
        public ConfigFile ConfigFile
        {
            get => _configFile;
            set
            {
                if (IsConfigured)
                    return;
                _configFile = value;
            }
        }
        private ConfigFile _configFile;


        /// <summary>
        /// The ConfigEntryBase of this ConfiguredVariable, usually set when <see cref="Configure"/> is called.
        /// </summary>
        public ConfigEntryBase ConfigEntryBase { get; protected set; }

        /// <summary>
        /// The Config Section for this ConfiguredVariable.
        /// <br>Cannot be null</br>
        /// <para>If the ConfiguredVariable is decorated by <see cref="AutoConfigAttribute"/>, a "Nicified" string of the Declaring Type's name is used.</para>
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true.</para>
        /// </summary>
        public string Section
        {
            get => _section;
            set
            {
                if (IsConfigured)
                    return;
                _section = value;
            }
        }
        private string _section;

        /// <summary>
        /// The Config Key for this ConfiguredVariable.
        /// <br>Cannot be null</br>
        /// <para>If the ConfiguredVariable is decorated by <see cref="AutoConfigAttribute"/>, a "Nicified" string of the Declaring Type's name is used.</para>
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true.</para>
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (IsConfigured)
                    return;
                _key = value;
            }
        }
        private string _key;

        /// <summary>
        /// The Config Description
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true.</para>
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (IsConfigured)
                    return;

                _description = value;
            }
        }
        private string _description;

        /// <summary>
        /// The ConfigFileIdentifier for this ConfigurableVariable
        /// <para>When provided, and <see cref="ConfigFile"/> is null, the ConfigFile is obtained using <see cref="ConfigSystem.GetConfigFile(string)"/></para>
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true.</para>
        /// </summary>
        public string ConfigFileIdentifier
        {
            get => _configFileIdentifier;
            set
            {
                if (!IsConfigured)
                    return;
                _configFileIdentifier = value;
            }
        }
        private string _configFileIdentifier;

        /// <summary>
        /// The ModGUID that owns this ConfiguredVariable, used mainly for Risk of Options implementation
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true</para>
        /// </summary>
        public string ModGUID
        {
            get => _modGUID;
            set
            {
                if (IsConfigured)
                    return;
                _modGUID = value;
            }
        }
        private string _modGUID;

        /// <summary>
        /// The Human Readable Name for <see cref="ModGUID"/>, used mainly for Risk of Options implementation
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true</para>
        /// </summary>
        public string ModName
        {
            get => _modName;
            set
            {
                if (IsConfigured)
                    return;
                _modName = value;
            }
        }
        private string _modName;


        /// <summary>
        /// Returns an unique hashcode for this ConfiguredVariable, using the HashCode of <see cref="Key"/> and <see cref="Section"/>
        /// </summary>
        public int ConfigHash => Convert.ToInt32(_key.GetHashCode() / 2) + (_section.GetHashCode() / 2);

        /// <summary>
        /// Wether the Variable is actually Configurable by the <see cref="BepInEx.Configuration.TomlTypeConverter"/>
        /// </summary>
        public bool CanBeConfigured { get; protected set; } = false;
        /// <summary>
        /// Wether this ConfigurableVariable has already been configued.
        /// <para>When set to true, most if not all properties for the ConfigruableVariable become ReadOnly</para>
        /// </summary>
        public bool IsConfigured { get; protected set; } = false;

        /// <summary>
        /// Implement the configuration of this ConfigurableVariable here
        /// </summary>
        public abstract void Configure();

        /// <summary>
        /// Chainable method for setting <see cref="ConfigFile"/>
        /// </summary>
        public ConfiguredVariable WithConfigFile(ConfigFile configFile)
        {
            ConfigFile = configFile;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="Section"/>
        /// </summary>
        public ConfiguredVariable WithSection(string section)
        {
            Section = section;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="Key"/>
        /// </summary>
        public ConfiguredVariable WithKey(string key)
        {
            Key = key;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="Description"/>
        /// </summary>
        public ConfiguredVariable WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ConfigFileIdentifier"/>
        /// </summary>
        public ConfiguredVariable WithConfigIdentifier(string identifier)
        {
            ConfigFileIdentifier = identifier;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ModGUID"/>
        /// </summary>
        public ConfiguredVariable WithModGUID(BaseUnityPlugin plugin)
        {
            ModGUID = plugin.Info.Metadata.GUID;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ModName"/>
        /// </summary>
        public ConfiguredVariable WithModName(BaseUnityPlugin plugin)
        {
            ModName = plugin.Info.Metadata.Name;
            return this;
        }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public ConfiguredVariable() { }

        /// <summary>
        /// Creates a new instance of <see cref="ConfiguredVariable"/> and determines wether the value can be configured by the <see cref="TomlTypeConverter"/>
        /// </summary>
        /// <param name="defaultValue">The default value for this variable</param>
        public ConfiguredVariable(object defaultValue)
        {
            CanBeConfigured = TomlTypeConverter.CanConvert(defaultValue.GetType());
        }

        /// <summary>
        /// When a <see cref="ConfiguredVariable"/> is decorated with this attribute, and the ConfigredVariable is in a public static field, the <see cref="ConfigSystem"/> will automatically configure the ConfiguredVariable using the field's default values, name and declaring type's name.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class AutoConfigAttribute : SearchableAttribute
        {
            /// <summary>
            /// An optional override for the ConfiguredVariable's Description.
            /// </summary>
            public string DescriptionOverride { get; set; }

            /// <summary>
            /// Returns a readable version of the attribute.
            /// </summary>
            public override string ToString()
            {
                MemberInfo memberInfo = (MemberInfo)target;
                return $"ConfiguredVariable attached to {memberInfo.DeclaringType.FullName}.{memberInfo.Name}";
            }
        }
    }

    /// <summary>
    /// A Generic version of <see cref="ConfiguredVariable"/>
    /// <para>The <see cref="ConfiguredVariable{T}"/> is a finished and working Variable that can be configured using the BepInEx Config System.</para>
    /// <para>Contains an implicit operator for casting the ConfigEntry's value into <typeparamref name="T"/></para>
    /// </summary>
    /// <typeparam name="T">The type of value that this ConfigurableVariable uses, for a list of valid types, see <see cref="TomlTypeConverter"/></typeparam>
    public class ConfiguredVariable<T> : ConfiguredVariable
    {
        private static Dictionary<int, DelegateContainer> configHashToDelegates = new Dictionary<int, DelegateContainer>();

        /// <summary>
        /// The default value for this ConfigurableVariable
        /// </summary>
        public T DefaultValue { get; } = default(T);

        /// <summary>
        /// The ConfigEntry for this ConfigurableVariable
        /// </summary>
        public ConfigEntry<T> ConfigEntry { get; private set; }

        /// <summary>
        /// The current and valid value of this ConfigurableVariable. Usually <see cref="ConfigEntry.Value"/>
        /// </summary>
        public T Value => ConfigEntry == null ? DefaultValue : ConfigEntry.Value;

        /// <summary>
        /// A delegate that gets invoked whenever the setting of <see cref="ConfigEntry"/> changes.
        /// <para>Once <see cref="ConfiguredVariable.IsConfigured"/> is true, the OnConfigChanged becomes readonly.</para>
        /// </summary>

        public event OnConfigChangedDelegate OnConfigChanged
        {
            add
            {
                if (IsConfigured)
                    return;
                _onConfigChanged += value;
            }
            remove
            {
                if (IsConfigured)
                    return;
                _onConfigChanged -= value;
            }
        }
        private OnConfigChangedDelegate _onConfigChanged;

        /// <summary>
        /// Returns the <see cref="DelegateContainer"/> for a ConfigEntry with the specified <paramref name="key"/> and <paramref name="section"/>
        /// </summary>
        /// <param name="key">The key value of the desired ConfigEntry</param>
        /// <param name="section">The desited section value of the desired ConfigEntry</param>
        /// <returns>The DelegateContainer for the ConfigEntry, if no DelegateContainer exists for said config it returns null.</returns>
        public static DelegateContainer GetDelegateContainer(string key, string section) => GetDelegateContainer(Convert.ToInt32(key.GetHashCode() / 2) + (section.GetHashCode() / 2));

        /// <summary>
        /// Returns the <see cref="DelegateContainer"/> for the specified ConfiguredVariable
        /// </summary>
        /// <returns>The DelegateContainer for the ConfigEntry, if no DelegateContainer exists for said config it returns null.</returns>
        public static DelegateContainer GetDelegateContainer(ConfiguredVariable configuredVariable) => GetDelegateContainer(configuredVariable.ConfigHash);

        /// <summary>
        /// <br>Returns the <see cref="DelegateContainer"/> for the specified <paramref name="configHash"/></br>
        /// </summary>
        /// <returns>The DelegateContainer for the ConfigEntry, if no DelegateContainer exists for said config it returns null.</returns>
        public static DelegateContainer GetDelegateContainer(int configHash)
        {
            if (configHashToDelegates.TryGetValue(configHash, out var delegateContainer))
                return delegateContainer;
            return null;
        }

        public override void Configure()
        {
            DoConfigure();
        }

        /// <summary>
        /// Chainable method for configuring this ConfiguredVariable
        /// </summary>
        public ConfiguredVariable<T> DoConfigure()
        {
            if (IsConfigured || !CanBeConfigured)
                return this;

            if(ConfigFile == null)
            {
                if(ConfigFileIdentifier.IsNullOrWhiteSpace())
                {
                    throw new NullReferenceException("ConfigFileIdentifier is Null, Empty or WhiteSpace");
                }

                var configFile = ConfigSystem.GetConfigFile(ConfigFileIdentifier);
                if (configFile == null)
                    throw new NullReferenceException("ConfigFile is null");

                ConfigFile = configFile;
            }

            if (Section.IsNullOrWhiteSpace())
                throw new NullReferenceException("Section is Null, Empty or WhiteSpace");

            if (Key.IsNullOrWhiteSpace())
                throw new NullReferenceException("Key is Null, Empty or WhiteSpace");

            ConfigEntry = ConfigFile.Bind(Section, Key, DefaultValue, Description);
            ConfigEntryBase = ConfigEntry;

            if(_onConfigChanged != null)
            {
                if (!configHashToDelegates.ContainsKey(ConfigHash))
                    configHashToDelegates[ConfigHash] = default(DelegateContainer);

                var val = new DelegateContainer();
                val.ConfigEntry = ConfigEntry;
                val.SetListeners(_onConfigChanged);
                val.Raise();
                configHashToDelegates[ConfigHash] = val;
            }
            IsConfigured = true;
            OnConfigured();
            return this;
        }

        /// <summary>
        /// A virtual method that gets called when the ConfiguredVariable is configured.
        /// </summary>
        protected virtual void OnConfigured() { }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigFile(ConfigFile)"/>
        public new ConfiguredVariable<T> WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithSection(ConfigFile)"/>
        public new ConfiguredVariable<T> WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(ConfigFile)"/>
        public new ConfiguredVariable<T> WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(ConfigFile)"/>
        public new ConfiguredVariable<T> WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(ConfigFile)"/>
        public new ConfiguredVariable<T> WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(ConfigFile)"/>
        public new ConfiguredVariable<T> WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(ConfigFile)"/>
        public new ConfiguredVariable<T> WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        /// <summary>
        /// Chainable method for adding a new <see cref="OnConfigChangedDelegate"/> to this ConfiguredVariable
        /// </summary>
        public ConfiguredVariable<T> WithConfigChange(OnConfigChangedDelegate del)
        {
            OnConfigChanged += del;
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.ConfiguredVariable(object)"/>
        public ConfiguredVariable(T defaultValue) : base(defaultValue)
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Implicitly casts the value of <paramref name="cf"/> to <typeparamref name="T"/>
        /// </summary>
        public static implicit operator T(ConfiguredVariable<T> cf) => cf.Value;

        /// <summary>
        /// A class that contains the Delegate implementations of a specified ConfigEntry.        
        /// /// <para>This is done so that ConfiguredVariables created on the fly and not stored in member fields can be garbage collected. without keeping all the unecesary data.</para>
        /// </summary>
        public class DelegateContainer
        {
            /// <summary>
            /// The ConfigEntry associated with this DelegateContainer
            /// </summary>
            public ConfigEntry<T> ConfigEntry
            {
                get => _configEntry;
                set
                {
                    if(_configEntry != null)
                    {
                        _configEntry.SettingChanged -= InvokeDelegates;
                    }
                    _configEntry = value;
                    _configEntry.SettingChanged += InvokeDelegates;
                }
            }
            private ConfigEntry<T> _configEntry;

            internal event OnConfigChangedDelegate OnConfigChanged;

            private void InvokeDelegates(object sender, EventArgs args)
            {
                Raise();
            }

            /// <summary>
            /// Manually raises the delegate and invokes any necesary methods.
            /// </summary>
            public void Raise()
            {
                if (ConfigEntry != null)
                    OnConfigChanged?.Invoke((T)ConfigEntry.BoxedValue);
            }


            internal void SetListeners(OnConfigChangedDelegate listeners)
            {
                OnConfigChanged = listeners;
            }
        }

        /// <summary>
        /// A delegate that gets called when the value in a ConfiguredVariable changes.
        /// </summary>
        /// <param name="newVal">The new variable for the Config Entry.</param>
        public delegate void OnConfigChangedDelegate(T newVal);
    }
}