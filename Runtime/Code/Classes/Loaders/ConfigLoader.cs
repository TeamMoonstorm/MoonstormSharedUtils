using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Moonstorm.Loaders
{
    /// <summary>
    /// The ConfigLoader is a class that can be used to simplify the implementation of ConfigFiles from BepInEx
    /// <para>ConfigLoader will easily create new Config files, config files created by it can be wiped between major versions</para>
    /// <para>ConfigLoader inheriting classes are treated as Singletons</para>
    /// </summary>
    /// <typeparam name="T">The class that's inheriting from ConfigLoader</typeparam>
    public abstract class ConfigLoader<T> : ConfigLoader where T : ConfigLoader<T>
    {
        /// <summary>
        /// Retrieves the instance of <typeparamref name="T"/>
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Parameterless Constructor for ConfigLoader, this will throw an invalid operation exception if an instance of <typeparamref name="T"/> already exists
        /// </summary>
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

    /// <summary>
    /// <inheritdoc cref="ConfigLoader{T}"/>
    /// <para>You probably want to use <see cref="ConfigLoader{T}"/> instead</para>
    /// </summary>
    public abstract class ConfigLoader
    {
        internal static List<ConfigLoader> instances = new List<ConfigLoader>();
        /// <summary>
        /// Your mod's main class
        /// </summary>
        public abstract BaseUnityPlugin MainClass { get; }
        /// <summary>
        /// Wether ConfigFiles created by the ConfigLoader will be created in a subfolder, or in the Bepinex's ConfigPath
        /// </summary>
        public abstract bool CreateSubFolder { get; }
        /// <summary>
        /// Returns the folder where the config files for this ConfigLoader are located
        /// </summary>
        public string ConfigFolderPath
        {
            get
            {
                return CreateSubFolder ? System.IO.Path.Combine(Paths.ConfigPath, OwnerMetaData.Name) : Paths.ConfigPath;
            }
        }
        /// <summary>
        /// Retrieves the MainClass's Owner Metadata
        /// </summary>
        public BepInPlugin OwnerMetaData { get => MainClass.Info.Metadata; }

        /// <summary>
        /// A dictionary to store a ConfigFile's identifier to its config file
        /// </summary>

        public Dictionary<string, ConfigFile> identifierToConfigFile = new Dictionary<string, ConfigFile>();

        /// <summary>
        /// Creates a config file.
        /// <para>The config file's name will be the <paramref name="identifier"/></para>
        /// </summary>
        /// <param name="identifier">A unique identifier for this config file</param>
        /// <param name="wipedBetweenMinorVersions">Wether the ConfigFile is wiped between minor version changes of your mod</param>
        /// <returns>The config file</returns>
        public ConfigFile CreateConfigFile(string identifier, bool wipedBetweenMinorVersions = true)
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

            identifierToConfigFile.Add(identifier, configFile);
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