using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace Moonstorm.Config
{
    /// <summary>
    /// A NonGeneric version of a <see cref="ConfigurableVariable{T}"/>, You're strongly advised to use <see cref="ConfigurableVariable{T}"/> instead.
    /// </summary>
    public abstract class ConfigurableVariable
    {
        /// <summary>
        /// The <see cref="ConfigFile"/> that this ConfigurableVariable is bound to.
        /// <para>If left null before the Configuration process, a ConfigFile is attempted to get using <see cref="ConfigSystem.GetConfigFile(string)"/> using <see cref="ConfigIdentifier"/></para>
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
        private ConfigFile _configFile = null;

        /// <summary>
        /// The ConfigEntryBase of this ConfigurableVariable, Usually Set when <see cref="Configure"/> is called
        /// </summary>
        public ConfigEntryBase ConfigEntryBase { get; protected set; }

        /// <summary>
        /// The Config Section for this ConfigurableVariable
        /// <para>If left Empty or Null, it'll use a "Nicified" string of the Declaring Type's name.</para>
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
        private string _section = string.Empty;

        /// <summary>
        /// The Config Key for this ConfigurableVariable
        /// <para>If left Empty or Null, it'll use a "Nicified" string of the Field/Property's name.</para>
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
        private string _key = string.Empty;

        /// <summary>
        /// Returns an unique hashcode for this ConfigurableVariable, using the HashCode of <see cref="Key"/> and <see cref="Section"/>
        /// </summary>
        public int ConfigHash
        {
            get
            {
                return Convert.ToInt32((Key.GetHashCode() / 2) + (Section.GetHashCode() / 2));
            }
        }

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
        private string _description = string.Empty;
        /// <summary>
        /// The ConfigFileIdentifier for this ConfigurableVariable
        /// <para>When provided, and <see cref="ConfigFile"/> is null, the ConfigFile is obtained using <see cref="ConfigSystem.GetConfigFile(string)"/></para>
        /// <para>Becomes ReadOnly if <see cref="IsConfigured"/> is true.</para>
        /// </summary>
        public string ConfigIdentifier
        {
            get => _configIdentifier;
            set
            {
                if (IsConfigured)
                    return;
                _configIdentifier = value;
            }
        }
        private string _configIdentifier = string.Empty;

        /// <summary>
        /// The ModGUID that owns this Config entry, used mainly for Risk of Options implementation
        /// <br>This is automatically set by the ConfigSystem and it's setter method exists for early initialization</br>
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
        private string _modGUID = string.Empty;

        /// <summary>
        /// The Human Readable Name for <see cref="ModGUID"/>, used mainly for Risk of Options implementation
        /// <br>This is automatically set by the ConfigSystem and it's setter method exists for early initialization</br>
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
        private string _modName = string.Empty;

        /// <summary>
        /// Wether the Variable is actually Configurable by the <see cref="BepInEx.Configuration.TomlTypeConverter"/>
        /// </summary>
        public bool IsActuallyConfigurable { get; } = false;

        /// <summary>
        /// Wether this ConfigurableVariable has already been configued.
        /// <para>When set to true, most if not all properties for the ConfigruableVariable become ReadOnly</para>
        /// </summary>
        public bool IsConfigured { get; protected set; } = false;

        /// <summary>
        /// Chainable method for setting <see cref="Section"/>
        /// </summary>
        public ConfigurableVariable SetSection(string section)
        {
            Section = section;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="Key"/>
        /// </summary>
        public ConfigurableVariable SetKey(string key)
        {
            Key = key;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="Description"/>
        /// </summary>
        public ConfigurableVariable SetDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ConfigIdentifier"/>
        /// </summary>
        public ConfigurableVariable SetIdentifier(string identifier)
        {
            ConfigIdentifier = identifier;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ConfigFile"/>
        /// </summary>
        public ConfigurableVariable SetConfigFile(ConfigFile file)
        {
            ConfigFile = file;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ModGUID"/>
        /// </summary>
        public ConfigurableVariable SetModGUID(string modGUID)
        {
            ModGUID = modGUID;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ModName"/>
        /// </summary>
        public ConfigurableVariable SetModName(string modName)
        {
            ModName = modName;
            return this;
        }

        /// <summary>
        /// Implement the configuration of this ConfigurableVariable here
        /// </summary>
        public abstract void Configure();

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurableVariable"/> and determines wether the value can be configured by the <see cref="TomlTypeConverter"/>
        /// </summary>
        /// <param name="defaultValue">The default value for this variable</param>
        public ConfigurableVariable(object defaultValue)
        {
            IsActuallyConfigurable = TomlTypeConverter.CanConvert(defaultValue.GetType());
        }
    }

    /// <summary>
    /// A Generic version of <see cref="ConfigurableVariable"/>
    /// <para>The <see cref="ConfigurableVariable{T}"/> is a finished and working Variable that can be configured using the BepInEx Config system.</para>
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <see cref="T"/></br>
    /// </summary>
    /// <typeparam name="T">The type of value that this ConfigurableVariable uses, for a list of valid types, see <see cref="TomlTypeConverter"/></typeparam>
    public class ConfigurableVariable<T> : ConfigurableVariable
    {
        /// <summary>
        /// Represents a container for delegates for a ConfigurableVariable. these are stored automatically by MSU and created when a ConfigurableVariable is bound.
        /// <br>This allows the ConfigurableVariable to be garbage collected if necesary, while keeping the OnConfigchanged functionality intact.</br>
        /// </summary>
        public class DelegateContainer
        {
            /// <summary>
            /// The ConfigEntry tied to this delegate container
            /// </summary>
            public ConfigEntry<T> Entry
            {
                get => _entry;
                internal set
                {
                    if (_entry != null)
                        _entry.SettingChanged -= InvokeDelegate;

                    _entry = value;
                    _entry.SettingChanged += InvokeDelegate;
                }
            }
            private ConfigEntry<T> _entry;

            internal event OnConfigChangedDelegate OnConfigChanged;

            private void InvokeDelegate(object sender, EventArgs args)
            {
                Raise();
            }

            /// <summary>
            /// Manually invokes the delegates tied to this DelegateContainer
            /// </summary>
            public void Raise()
            {
                if (Entry != null)
                    OnConfigChanged?.Invoke((T)Entry.BoxedValue);
            }

            internal void SetListeners(OnConfigChangedDelegate listeners)
            {
                OnConfigChanged = listeners;
            }
        }
        public delegate void OnConfigChangedDelegate(T newVal);

        private static Dictionary<int, DelegateContainer> configHashToDelegates = new Dictionary<int, DelegateContainer>();

        /// <summary>
        /// Returns the DelegateContainer tied to a specific ConfigHash
        /// </summary>
        /// <param name="configHash">The ConfigHash of the delegate, this can be obtained from <see cref="ConfigurableVariable.ConfigHash"/></param>
        /// <returns>The delegate container for the config, null if no delegate container exists.</returns>
        public static DelegateContainer GetDelegateContainer(int configHash)
        {
            if (configHashToDelegates.TryGetValue(configHash, out DelegateContainer val))
                return val;
            return null;
        }

        /// <summary>
        /// The default value for this ConfigurableVariable
        /// </summary>
        public T DefaultValue { get; } = default(T);

        /// <summary>
        /// The current and valid value of this ConfigurableVariable. Usually <see cref="ConfigEntry.Value"/>
        /// </summary>
        public T Value => ConfigEntry == null ? DefaultValue : ConfigEntry.Value;

        /// <summary>
        /// The ConfigEntry for this ConfigurableVariable
        /// </summary>
        public ConfigEntry<T> ConfigEntry { get; private set; }

        /// <summary>
        /// A delegate that gets invoked whenever the setting of <see cref="ConfigEntry"/> changes.
        /// <para>Once <see cref="ConfigurableVariable.IsConfigured"/> is true, the OnConfigChanged becomes readonly.</para>
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
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableVariable<T> SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableVariable<T> SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableVariable<T> SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableVariable<T> SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableVariable<T> SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableVariable<T> SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableVariable<T> SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        /// <summary>
        /// A Chainable method for Adding to <see cref="OnConfigChanged"/>
        /// </summary>
        public ConfigurableVariable<T> AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            OnConfigChanged += onConfigChanged;
            return this;
        }

        /// <summary>
        /// Calls <see cref="DoConfigure"/> and returns void. You're strongly advised to call <see cref="DoConfigure"/> instead.
        /// </summary>
        public override void Configure()
        {
            DoConfigure();
        }

        /// <summary>
        /// Chainable method that configures this ConfigurableVariable using the specified data. This is normally called automatically by the <see cref="ConfigSystem"/>, but it can be used for early initialization of configs if need be.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public ConfigurableVariable<T> DoConfigure()
        {
            if (IsConfigured || !IsActuallyConfigurable)
                return this;


            if (ConfigFile == null)
            {
                if (ConfigIdentifier == null)
                    throw new NullReferenceException("ConfigIdentifier is null");

                var configFile = ConfigSystem.GetConfigFile(ConfigIdentifier);
                if (configFile == null)
                    throw new NullReferenceException("ConfigFile is null");

                ConfigFile = configFile;
            }
            if (Section.IsNullOrWhiteSpace())
                throw new NullReferenceException("Section is null, empty or whitespace");
            if (Key.IsNullOrWhiteSpace())
                throw new NullReferenceException("Key is null, empty or whitespace");

            ConfigEntry = ConfigFile.Bind(Section, Key, DefaultValue, Description);
            ConfigEntryBase = ConfigEntry;
            if (_onConfigChanged != null)
            {
                if (!configHashToDelegates.ContainsKey(ConfigHash))
                    configHashToDelegates[ConfigHash] = default(DelegateContainer);

                var val = new DelegateContainer();
                val.Entry = ConfigEntry;
                val.SetListeners(_onConfigChanged);
                val.Raise();
                configHashToDelegates[ConfigHash] = val;
            }
            IsConfigured = true;
            OnConfigured();
            return this;
        }

        /// <summary>
        /// When <see cref="ConfigurableVariable{T}.DoConfigure"/> is called and <see cref="ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable
        /// </summary>
        protected virtual void OnConfigured()
        {

        }

        /// <summary>
        /// Returns a readable string of this Configurable Variable
        /// </summary>
        /// <returns>A readable string</returns>
        public override string ToString()
        {
            return $"{GetType().Name} (Configured Type: {typeof(T).Name}, Default Value: {DefaultValue}, Configured Value: {Value}, ConfigDefinition: {Section}, {Key})";
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.ConfigurableVariable(object)"/>
        /// </summary>
        /// <param name="defaultVal"><inheritdoc cref="ConfigurableVariable.ConfigurableVariable(object)"/></param>
        public ConfigurableVariable(T defaultVal) : base(defaultVal)
        {
            DefaultValue = defaultVal;
        }

        /// <summary>
        /// An implicit operator that casts the ConfigurableVariable's <see cref="ConfigEntry.Value"/> into <typeparamref name="T"/>
        /// </summary>
        /// <param name="cf">The instance of the ConfigurableVariable</param>
        public static implicit operator T(ConfigurableVariable<T> cf) => cf.Value;
    }
}
