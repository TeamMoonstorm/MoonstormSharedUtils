using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using Moonstorm;
using System;

namespace Moonstorm.Config
{
    public class ConfigurableVariable<T>
    {
        public const string DEFAULT_CONFIG_FILE = "DefaultModConfig";
        public T DefaultValue { get; } = default(T);
        public ConfigFile ConfigFile { get; set; }
        public ConfigEntry<T> InternalEntry { get; private set; }
        public string Section { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public string ConfigIdentifier { get; set; }
        public bool IsActuallyConfigurable { get; } = false;
        public bool IsConfigured { get; private set; } = false;

        public ConfigurableVariable<T> SetSection(string section)
        {
            if (IsConfigured)
                return this;

            Section = section;
            return this;
        }

        public ConfigurableVariable<T> SetKey(string key)
        {
            if (IsConfigured)
                return this;

            Key = key;
            return this;
        }

        public ConfigurableVariable<T> SetDescription(string description)
        {
            if (IsConfigured)
                return this;

            Description = description;
            return this;
        }

        public ConfigurableVariable<T> SetIdentifier(string identifier)
        {
            if (IsConfigured)
                return this;

            ConfigIdentifier = identifier;
            return this;
        }

        public ConfigurableVariable<T> SetConfigFile(ConfigFile file)
        {
            if (IsConfigured)
                return this;

            ConfigFile = file;
            return this;
        }

        public ConfigurableVariable<T> Configure()
        {
            if (IsConfigured || !IsActuallyConfigurable)
                return this;


            if (ConfigFile == null)
            {
                if(ConfigIdentifier == null)
                    throw new NullReferenceException("ConfigIdentifier is null");

                var configFile = ConfigurableFieldManager.GetConfigFile(ConfigIdentifier);
                if (configFile == null)
                    throw new NullReferenceException("ConfigFile is null");

                ConfigFile = configFile;
            }
            if (Section == null)
                throw new NullReferenceException("Section is null");
            if (Key == null)
                throw new NullReferenceException("Key is null");
            if (Description == null)
                throw new NullReferenceException("Description is null");

            InternalEntry = ConfigFile.Bind(Section, Key, DefaultValue, Description);
            IsConfigured = true;
            OnConfigured();
            return this;
        }

        protected virtual void OnConfigured()
        {

        }

        public ConfigurableVariable(T defaultVal)
        {
            IsActuallyConfigurable = TomlTypeConverter.CanConvert(typeof(T));
            DefaultValue = defaultVal;
#if DEBUG
            CheckConfigurability(this);
#endif
        }

        public static implicit operator T(ConfigurableVariable<T> cf) => cf.InternalEntry.Value; 

#if DEBUG
        private static void CheckConfigurability(ConfigurableVariable<T> instance)
        {
            if(instance.IsActuallyConfigurable)
            {
                MSULog.Error($"The Type specified in ConfigurableField<T> is not a configurable type by the bepinex config system.");
            }
        }
#endif
    }
}
