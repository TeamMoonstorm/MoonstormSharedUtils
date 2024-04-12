using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.IsConfigured"/> is true</para>
        /// </summary>
        public CheckBoxConfig CheckBoxConfig
        {
            get => _checkBoxConfig;
            set
            {
                if (IsConfigured)
                    return;

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

        ///<inheritdoc cref="ConfiguredVariable.WithSection(ConfigFile)"/>
        public new ConfiguredBool WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(ConfigFile)"/>
        public new ConfiguredBool WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(ConfigFile)"/>
        public new ConfiguredBool WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(ConfigFile)"/>
        public new ConfiguredBool WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(ConfigFile)"/>
        public new ConfiguredBool WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(ConfigFile)"/>
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
        /// Chainable method for setting <see cref="CheckBoxConfig"/>
        /// </summary>

        public ConfiguredBool WithCheckBoxConfig(CheckBoxConfig config)
        {
            CheckBoxConfig = config;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
            {
                bool separateEntry = ConfigSystem.ShouldCreateSeparateRiskOfOptionsEntry(ConfigFile);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(ConfigFile.ConfigFilePath);
                var guid = separateEntry ? ModGUID + "." + fileName : ModGUID;
                var name = separateEntry ? ModName + "." + fileName : ModName;
                CheckBoxOption option = CheckBoxConfig == null ? new CheckBoxOption(ConfigEntry) : new CheckBoxOption(ConfigEntry, CheckBoxConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <inheritdoc cref="ConfiguredVariable.ConfiguredVariable(object)"/>
        public ConfiguredBool(bool defaultVal) : base(defaultVal) { }
    }
}