using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU.Config
{
    /// <summary>
    /// The ConfigFactory is a class that can be used to simplify the management of Configuration options from BepInEx.
    /// <br>It can create new config files that can be stored in a separate subfolder for easy management, as well as easily creating new instances of <see cref="ConfiguredVariable{T}"/></br>
    /// </summary>
    public class ConfigFactory
    {
        /// <summary>
        /// Returns the path to the folder that contains a mod's Config Folder.
        /// </summary>
        public string configFolderPath
        {
            get
            {
                return _createSubFolder ? System.IO.Path.Combine(Paths.ConfigPath, _plugin.Info.Metadata.Name) : Paths.ConfigPath;
            }
        }
        private BaseUnityPlugin _plugin;
        private bool _createSubFolder;

        /// <summary>
        /// Creates a new ConfigFile with the identifier specified in <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">The identifier to use for this ConfigFile, the identifier will also be the name of the ConfigFile</param>
        /// <param name="createSeparateRiskOfOptionsEntry">If true, MSU will create a spearate Risk of Options entry for any Configurations associated by the created ConfigFile.</param>
        /// <returns>The new ConfigFile.</returns>
        public ConfigFile CreateConfigFile(string identifier, bool createSeparateRiskOfOptionsEntry = false)
        {
            string fileName = identifier;
            if(!fileName.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".cfg";
            }
            var path = System.IO.Path.Combine(configFolderPath, fileName);
            ConfigFile configFile = new ConfigFile(path, true, _plugin.Info.Metadata);

            ConfigSystem.AddConfigFileAndIdentifier(identifier, configFile, _plugin, createSeparateRiskOfOptionsEntry);
            return configFile;
        }

        /// <summary>
        /// Creates a ConfiguredVariable of type <typeparamref name="TVal"/> and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <typeparam name="TVal">The type that the configurable variable will configure</typeparam>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredVariable</returns>
        public ConfiguredVariable<TVal> MakeConfiguredVariable<TVal>(TVal defaultVal, Action<ConfiguredVariable<TVal>> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredVariable<TVal>(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a ConfiguredBool and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredBool</returns>
        public ConfiguredBool MakeConfiguredBool(bool defaultVal, Action<ConfiguredBool> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredBool(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a ConfiguredColor and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredColor</returns>
        public ConfiguredColor MakeConfiguredColor(Color defaultVal, Action<ConfiguredColor> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredColor(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a ConfiguredEnum of type <typeparamref name="TEnum"/> and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredEnum</returns>
        public ConfiguredEnum<TEnum> MakeConfiguredEnum<TEnum>(TEnum defaultVal, Action<ConfiguredEnum<TEnum>> initializer = null) where TEnum : Enum
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredEnum<TEnum>(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a ConfiguredFloat and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredFloat</returns>
        public ConfiguredFloat MakeConfiguredFloat(float defaultVal, Action<ConfiguredFloat> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredFloat(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a ConfiguredInt and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredInt</returns>
        public ConfiguredInt MakeConfiguredInt(int defaultVal, Action<ConfiguredInt> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredInt(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a ConfiguredKeyBind and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredKeyBind</returns>
        public ConfiguredKeyBind MakeConfiguredKeyBind(KeyboardShortcut defaultVal, Action<ConfiguredKeyBind> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredKeyBind(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a ConfiguredString and automatically sets it's <see cref="ConfiguredVariable.modGUID"/> and <see cref="ConfiguredVariable.modName"/> to the mod responsible for creating the ConfigFactory.
        /// </summary>
        /// <param name="defaultVal">The default value for the variable</param>
        /// <param name="initializer">Optional initializer</param>
        /// <returns>The created ConfiguredString</returns>
        public ConfiguredString MakeConfiguredString(string defaultVal, Action<ConfiguredString> initializer = null)
        {
            var metadata = _plugin.Info.Metadata;
            var cfg = new ConfiguredString(defaultVal)
            {
                modGUID = metadata.GUID,
                modName = metadata.Name
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        /// <summary>
        /// Creates a new ConfigFactory with the specified BaseUnityPlugin as the mod responsible for creating it.
        /// </summary>
        /// <param name="baseUnityPlugin">The base unity plugin that's responsible for this ConfigFactory</param>
        public ConfigFactory(BaseUnityPlugin baseUnityPlugin)
        {
            _plugin = baseUnityPlugin;
            _createSubFolder = false;
        }

        /// <summary>
        /// Creates a new ConfigFactory with the specified BaseUnityPlugin as the mod responsible for creating it.
        /// </summary>
        /// <param name="baseUnityPlugin">The base unity plugin that's responsible for this ConfigFactory</param>
        /// <param name="createSubFolder">Wether a subfolder should be created for any new ConfigFiles created by this ConfigFactory.</param>
        public ConfigFactory(BaseUnityPlugin baseUnityPlugin, bool createSubFolder)
        {
            _plugin = baseUnityPlugin;
            _createSubFolder = createSubFolder;
        }
    }
}