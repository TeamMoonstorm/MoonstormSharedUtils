using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU.Config
{
    public class ConfigFactory
    {
        public string ConfigFolderPath
        {
            get
            {
                return _createSubFolder ? System.IO.Path.Combine(Paths.ConfigPath, _plugin.Info.Metadata.Name) : Paths.ConfigPath;
            }
        }
        private BaseUnityPlugin _plugin;
        private bool _createSubFolder;

        public ConfigFile CreateConfigFile(string identifier, bool createSeparateRiskOfOptionsEntry = false)
        {
            string fileName = identifier;
            if(!fileName.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".cfg";
            }
            var path = System.IO.Path.Combine(ConfigFolderPath, fileName);
            ConfigFile configFile = new ConfigFile(path, true, _plugin.Info.Metadata);

            ConfigSystem.AddConfigFileAndIdentifier(identifier, configFile, _plugin, createSeparateRiskOfOptionsEntry);
            return configFile;
        }

        public ConfiguredVariable<TVal> MakeConfiguredVariable<TVal>(TVal defaultVal, Action<ConfiguredVariable<TVal>> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredVariable<TVal>(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfiguredBool MakeConfiguredBool(bool defaultVal, Action<ConfiguredBool> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredBool(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfiguredColor MakeConfiguredColor(Color defaultVal, Action<ConfiguredColor> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredColor(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfiguredEnum<TEnum> MakeConfiguredEnum<TEnum>(TEnum defaultVal, Action<ConfiguredEnum<TEnum>> initializer = null) where TEnum : Enum
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredEnum<TEnum>(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfiguredFloat MakeConfiguredFloat(float defaultVal, Action<ConfiguredFloat> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredFloat(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfiguredInt MakeConfiguredInt(int defaultVal, Action<ConfiguredInt> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredInt(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfiguredKeyBind MakeConfiguredKeyBind(KeyboardShortcut defaultVal, Action<ConfiguredKeyBind> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredKeyBind(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfiguredString MakeConfiguredString(string defaultVal, Action<ConfiguredString> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredString(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public ConfigFactory(BaseUnityPlugin baseUnityPlugin)
        {
            _plugin = baseUnityPlugin;
            _createSubFolder = true;
        }
        public ConfigFactory(BaseUnityPlugin baseUnityPlugin, bool createSubFolder)
        {
            _plugin = baseUnityPlugin;
            _createSubFolder = createSubFolder;
        }
    }
}