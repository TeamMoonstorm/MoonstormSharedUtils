using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using Moonstorm;
using System;
using BepInEx;

namespace Moonstorm.Config
{
    /// <summary>
    /// A NonGeneric version of a ConfigurableVariable, You're strongly advised to use <see cref="ConfigurableVariable{T}"/> instead.
    /// </summary>
    public abstract class ConfigurableVariable
    {
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
        public string ModGUID { get; internal set; } = string.Empty;
        public string ModName { get; internal set; } = string.Empty;
        public bool IsActuallyConfigurable { get; } = false;
        public bool IsConfigured { get; protected set; } = false;
        public ConfigurableVariable SetSection(string section)
        {
            if (IsConfigured)
                return this;

            Section = section;
            return this;
        }

        public ConfigurableVariable SetKey(string key)
        {
            if (IsConfigured)
                return this;

            Key = key;
            return this;
        }

        public ConfigurableVariable SetDescription(string description)
        {
            if (IsConfigured)
                return this;

            Description = description;
            return this;
        }

        public ConfigurableVariable SetIdentifier(string identifier)
        {
            if (IsConfigured)
                return this;

            ConfigIdentifier = identifier;
            return this;
        }

        public ConfigurableVariable SetConfigFile(ConfigFile file)
        {
            if (IsConfigured)
                return this;

            ConfigFile = file;
            return this;
        }

        internal abstract void Configure();
        public ConfigurableVariable(object defaultValue)
        {
            IsActuallyConfigurable = TomlTypeConverter.CanConvert(defaultValue.GetType());
        }
    }

    public class ConfigurableVariable<T> : ConfigurableVariable
    {
        public T DefaultValue { get; } = default(T);
        public T Value => ConfigEntry == null ? default(T) : ConfigEntry.Value;
        public ConfigEntry<T> ConfigEntry { get; private set; }
        
        public new ConfigurableVariable<T> SetSection(string section)
        {
            if (IsConfigured)
                return this;

            Section = section;
            return this;
        }

        public new ConfigurableVariable<T> SetKey(string key)
        {
            if (IsConfigured)
                return this;

            Key = key;
            return this;
        }

        public new ConfigurableVariable<T> SetDescription(string description)
        {
            if (IsConfigured)
                return this;

            Description = description;
            return this;
        }

        public new ConfigurableVariable<T> SetIdentifier(string identifier)
        {
            if (IsConfigured)
                return this;

            ConfigIdentifier = identifier;
            return this;
        }

        public new ConfigurableVariable<T> SetConfigFile(ConfigFile file)
        {
            if (IsConfigured)
                return this;

            ConfigFile = file;
            return this;
        }

        internal override void Configure()
        {
            DoConfigure();
        }
        public ConfigurableVariable<T> DoConfigure()
        {
            if (IsConfigured || !IsActuallyConfigurable)
                return this;


            if (ConfigFile == null)
            {
                if(ConfigIdentifier == null)
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
            IsConfigured = true;
            OnConfigured();
            return this;
        }

        protected virtual void OnConfigured()
        {

        }

        public ConfigurableVariable(T defaultVal) : base(defaultVal)
        {
            DefaultValue = defaultVal;
        }

        public static implicit operator T(ConfigurableVariable<T> cf) => cf.ConfigEntry.Value; 
    }
}
