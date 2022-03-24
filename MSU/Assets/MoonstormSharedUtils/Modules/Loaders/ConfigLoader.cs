using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

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
                instances.Add(Instance);
            }
            catch (Exception e) { MSULog.Error(e); }
        }
    }

    public abstract class ConfigLoader
    {
        internal static List<ConfigLoader> instances = new List<ConfigLoader>();
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

        public Dictionary<string, ConfigFile> identifierToConfigFile = new Dictionary<string, ConfigFile>();

        public ConfigFile CreateConfigFile(string identifier, bool wipedBetweenMinorVersions = true)
        {
            if(!identifier.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
            {
                identifier += ".cfg";
            }
            var path = System.IO.Path.Combine(ConfigFolderPath, identifier);
            ConfigFile configFile = new ConfigFile(path, true, OwnerMetaData);
            if (wipedBetweenMinorVersions)
                TryWipeConfig(configFile);
            return configFile;
        }

        private void TryWipeConfig(ConfigFile configFile)
        {
            ConfigDefinition configDef = new ConfigDefinition("Version", "Config File Version");
            string configVersionValue = $"{OwnerMetaData.Version.Major}.{OwnerMetaData.Version.Minor}";
            ConfigEntry<string> versionEntry = null;
            if(configFile.TryGetEntry<string>(configDef, out versionEntry))
            {
                string currentValue = versionEntry.Value;

                if(currentValue != configVersionValue)
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