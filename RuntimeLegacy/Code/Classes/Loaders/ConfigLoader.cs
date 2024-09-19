using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Config;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Moonstorm.Loaders
{
    public abstract class ConfigLoader<T> : ConfigLoader where T : ConfigLoader<T>
    {
        public static T Instance { get; private set; }

        public ConfigLoader()
        {
            try
            {
                if (Instance != null)
                {
                    throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ConfigLoader was instantiated twice");
                }
                Instance = this as T;
            }
            catch (Exception e) { MSULog.Error(e); }
        }

        public static ConfigurableVariable<TVal> MakeConfigurableVariable<TVal>(TVal defaultVal, Action<ConfigurableVariable<TVal>> initializer = null)
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableVariable<TVal>)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableVariable<TVal>(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public static ConfigurableBool MakeConfigurableBool(bool defaultVal, Action<ConfigurableBool> initializer = null)
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableBool)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableBool(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public static ConfigurableColor MakeConfigurableColor(Color defaultVal, Action<ConfigurableColor> initializer = null)
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableColor)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableColor(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public static ConfigurableEnum<TEnum> MakeConfigurableEnum<TEnum>(TEnum defaultVal, Action<ConfigurableEnum<TEnum>> initializer = null) where TEnum : Enum
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableEnum<TEnum>)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableEnum<TEnum>(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public static ConfigurableFloat MakeConfigurableFloat(float defaultVal, Action<ConfigurableFloat> initializer = null)
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableFloat)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableFloat(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public static ConfigurableInt MakeConfigurableInt(int defaultVal, Action<ConfigurableInt> initializer = null)
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableInt)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableInt(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }


        public static ConfigurableString MakeConfigurableString(string defaultVal, Action<ConfigurableString> initializer = null)
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableString)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableString(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        public static ConfigurableKeyBind MakeConfigurableKeyBind(KeyboardShortcut defaultVal, Action<ConfigurableKeyBind> initializer = null)
        {
            ThrowIfNoInstance($"Create {nameof(ConfigurableKeyBind)}");

            var metadata = Instance.MainClass.Info.Metadata;
            var cfg = new ConfigurableKeyBind(defaultVal)
            {
                ModGUID = metadata.GUID,
                ModName = metadata.Name,
            };
            initializer?.Invoke(cfg);
            return cfg;
        }

        protected static void ThrowIfNoInstance(string attemptedAction)
        {
#if !UNITY_EDITOR
            if (Instance == null)
                throw new NullReferenceException($"Cannot {attemptedAction} when there is no instance of {typeof(T).Name}!");
#endif
        }
    }

    public abstract class ConfigLoader
    {

        public abstract BaseUnityPlugin MainClass { get; }

        public abstract bool CreateSubFolder { get; }

        public string ConfigFolderPath
        {
            get
            {
                return CreateSubFolder ? System.IO.Path.Combine(Paths.ConfigPath, OwnerMetaData.Name) : Paths.ConfigPath;
            }
        }

        public BepInPlugin OwnerMetaData { get => MainClass.Info.Metadata; }

        public ConfigFile CreateConfigFile(string identifier, bool wipedBetweenMinorVersions = true)
        {
            return CreateConfigFile(identifier, wipedBetweenMinorVersions, false);
        }

        public ConfigFile CreateConfigFile(string identifier, bool wipedBetweenMinorVersions = true, bool createSeparateRooEntry = false)
        {
            string fileName = identifier;
            if (!fileName.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".cfg";
            }
            var path = System.IO.Path.Combine(ConfigFolderPath, fileName);
            ConfigFile configFile = new ConfigFile(path, true, OwnerMetaData);
            if (wipedBetweenMinorVersions)
                TryWipeConfig(configFile);

            ConfigSystem.AddConfigFileAndIdentifier(identifier, configFile, createSeparateRooEntry);
            return configFile;
        }

        private void TryWipeConfig(ConfigFile configFile)
        {
            ConfigDefinition configDef = new ConfigDefinition("Version", "Config File Version");
            string configVersionValue = $"{OwnerMetaData.Version.Major}.{OwnerMetaData.Version.Minor}";
            ConfigEntry<string> versionEntry = null;
            if (configFile.TryGetEntry<string>(configDef, out versionEntry))
            {
                string currentValue = versionEntry.Value;

                if (currentValue != configVersionValue)
                {
                    WipeConfig(configFile);
                    versionEntry.Value = configVersionValue;
                }
                return;
            }
            configFile.Bind<string>("Version", "Config File Version", $"{OwnerMetaData.Version.Major}.{OwnerMetaData.Version.Minor}", "Version of this ConfigFile, do not change this value.");
        }

        private void WipeConfig(ConfigFile configFile)
        {
            configFile.Clear();

            var orphanedEntriesProp = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile);
            orphanedEntries.Clear();

            configFile.Save();
        }
    }
}