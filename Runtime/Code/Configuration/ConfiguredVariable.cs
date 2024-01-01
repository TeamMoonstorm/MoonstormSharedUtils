using BepInEx;
using BepInEx.Configuration;
using HG.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSU.Config
{
    public abstract class ConfiguredVariable
    {
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class AutoConfigAttribute : SearchableAttribute
        {
            public string DescriptionOverride { get; set; }

            public override string ToString()
            {
                MemberInfo memberInfo = (MemberInfo)target;
                return $"ConfiguredVariable attached to {memberInfo.DeclaringType.FullName}.{memberInfo.Name}";
            }
        }
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

        public ConfigEntryBase ConfigEntryBase { get; protected set; }

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

        public int ConfigHash => Convert.ToInt32(_key.GetHashCode() / 2) + (_section.GetHashCode() / 2);

        public bool CanBeConfigured { get; protected set; } = false;
        public bool IsConfigured { get; protected set; } = false;

        public abstract void Configure();

        public ConfiguredVariable WithConfigFile(ConfigFile configFile)
        {
            ConfigFile = configFile;
            return this;
        }

        public ConfiguredVariable WithSection(string section)
        {
            Section = section;
            return this;
        }

        public ConfiguredVariable WithKey(string key)
        {
            Key = key;
            return this;
        }

        public ConfiguredVariable WithDescription(string description)
        {
            Description = description;
            return this;
        }

        public ConfiguredVariable WithConfigIdentifier(string identifier)
        {
            ConfigFileIdentifier = identifier;
            return this;
        }

        public ConfiguredVariable WithModGUID(BaseUnityPlugin plugin)
        {
            ModGUID = plugin.Info.Metadata.GUID;
            return this;
        }

        public ConfiguredVariable WithModName(BaseUnityPlugin plugin)
        {
            ModName = plugin.Info.Metadata.Name;
            return this;
        }

        public ConfiguredVariable() { }
        public ConfiguredVariable(object defaultValue)
        {
            CanBeConfigured = TomlTypeConverter.CanConvert(defaultValue.GetType());
        }
    }

    public class ConfiguredVariable<T> : ConfiguredVariable
    {
        private static Dictionary<int, DelegateContainer> configHashToDelegates = new Dictionary<int, DelegateContainer>();

        public T DefaultValue { get; } = default(T);
        public ConfigEntry<T> ConfigEntry { get; private set; }
        public T Value => ConfigEntry == null ? DefaultValue : ConfigEntry.Value;

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

        public static DelegateContainer GetDelegateContainer(string key, string section) => GetDelegateContainer(Convert.ToInt32(key.GetHashCode() / 2) + (section.GetHashCode() / 2));

        public static DelegateContainer GetDelegateContainer(ConfiguredVariable configuredVariable) => GetDelegateContainer(configuredVariable.ConfigHash);

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

        protected virtual void OnConfigured() { }

        public new ConfiguredVariable<T> WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredVariable<T> WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredVariable<T> WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredVariable<T> WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredVariable<T> WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredVariable<T> WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredVariable<T> WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public ConfiguredVariable<T> WithConfigChange(OnConfigChangedDelegate del)
        {
            OnConfigChanged += del;
            return this;
        }

        public ConfiguredVariable(T defaultValue) : base(defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public static implicit operator T(ConfiguredVariable<T> cf) => cf.Value;

        public class DelegateContainer
        {
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
        public delegate void OnConfigChangedDelegate(T newVal);
    }
}