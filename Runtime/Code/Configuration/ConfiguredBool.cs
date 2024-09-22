using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace MSU.Config
{
    /// <summary>
    /// <inheritdoc cref="ConfiguredVariable{T}"/>
    /// <br>Also contains necesary info for implementing the Config with Risk of Options.</br>
    /// <br>T is bool</br>
    /// </summary>
    public class ConfiguredBool : ConfiguredVariable<bool>
    {
        /// <summary>
        /// An optional CheckBoxConfig for this ConfiguredBool's <see cref="RiskOfOptions.Options.CheckBoxOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.isConfigured"/> is true</para>
        /// </summary>
        public CheckBoxConfig checkBoxConfig
        {
            get => _checkBoxConfig;
            set
            {
                if (isConfigured)
                {
#if DEBUG
                    LogReadOnly(nameof(checkBoxConfig));
#endif
                    return;
                }

                _checkBoxConfig = value;
            }
        }
        private CheckBoxConfig _checkBoxConfig;

        /// <inheritdoc cref="ConfiguredVariable{T}.DoConfigure"/>
        public new ConfiguredBool DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigFile(ConfigFile)"/>
        public new ConfiguredBool WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithSection(string)"/>
        public new ConfiguredBool WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(string)"/>
        public new ConfiguredBool WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(string)"/>
        public new ConfiguredBool WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(string)"/>
        public new ConfiguredBool WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(BaseUnityPlugin)"/>
        public new ConfiguredBool WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(BaseUnityPlugin)"/>
        public new ConfiguredBool WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        /// <inheritdoc cref="ConfiguredVariable{T}.WithConfigChange(ConfiguredVariable{T}.OnConfigChangedDelegate)"/>
        public new ConfiguredBool WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="checkBoxConfig"/>
        /// </summary>

        public ConfiguredBool WithCheckBoxConfig(CheckBoxConfig config)
        {
            checkBoxConfig = config;
            return this;
        }
        /// <inheritdoc/>

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(modGUID.IsNullOrWhiteSpace() || modName.IsNullOrWhiteSpace()))
            {
                bool separateEntry = ConfigSystem.ShouldCreateSeparateRiskOfOptionsEntry(configFile);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
                var guid = separateEntry ? modGUID + "." + fileName : modGUID;
                var name = separateEntry ? modName + "." + fileName : modName;
                CheckBoxOption option = checkBoxConfig == null ? new CheckBoxOption(configEntry) : new CheckBoxOption(configEntry, checkBoxConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <inheritdoc cref="ConfiguredVariable.ConfiguredVariable(object)"/>
        public ConfiguredBool(bool defaultVal) : base(defaultVal) { }
    }
}