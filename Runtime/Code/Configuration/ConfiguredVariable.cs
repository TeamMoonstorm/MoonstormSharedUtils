using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MSU.Config
{
    public abstract class ConfiguredVariable
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

        public ConfiguredVariable(object defaultValue)
        {
            CanBeConfigured = TomlTypeConverter.CanConvert(defaultValue.GetType());
        }
    }

    public class ConfiguredVariable<T> : ConfiguredVariable
    {
        public ConfiguredVariable(T defaultValue) : base(defaultValue)
        {
        }

        public override void Configure()
        {
        }

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
            }
        }
            public delegate void OnConfigChangedDelegate(DelegateContainer sender, T newVal, T previousVal);
    }
}