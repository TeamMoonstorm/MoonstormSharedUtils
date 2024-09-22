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
    /// <br>T is int</br>
    /// </summary>
    public class ConfiguredInt : ConfiguredVariable<int>
    {
        /// <summary>
        /// An optional IntSliderConfig for this ConfiguredInt's <see cref="RiskOfOptions.Options.IntSliderOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.isConfigured"/> is true</para>
        /// </summary>
        public IntSliderConfig sliderConfig
        {
            get => _sliderConfig;
            set
            {
                if (isConfigured)
                {
#if DEBUG
                    LogReadOnly(nameof(sliderConfig));
#endif
                    return;
                }
                _sliderConfig = value;
            }
        }
        private IntSliderConfig _sliderConfig;

        /// <inheritdoc cref="ConfiguredVariable{T}.DoConfigure"/>
        public new ConfiguredInt DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigFile(ConfigFile)"/>
        public new ConfiguredInt WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithSection(string)"/>
        public new ConfiguredInt WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(string)"/>
        public new ConfiguredInt WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(string)"/>
        public new ConfiguredInt WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(string)"/>
        public new ConfiguredInt WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(BaseUnityPlugin)"/>
        public new ConfiguredInt WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(BaseUnityPlugin)"/>
        public new ConfiguredInt WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        /// <inheritdoc cref="ConfiguredVariable{T}.WithConfigChange(ConfiguredVariable{T}.OnConfigChangedDelegate)"/>
        public new ConfiguredInt WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="sliderConfig"/>
        /// </summary>
        public ConfiguredInt WithIntSliderConfig(IntSliderConfig config)
        {
            sliderConfig = config;
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
                var option = sliderConfig == null ? new IntSliderOption(configEntry) : new IntSliderOption(configEntry, sliderConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <inheritdoc cref="ConfiguredVariable(object)"/>
        public ConfiguredInt(int value) : base(value) { }
    }
}