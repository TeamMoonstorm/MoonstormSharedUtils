using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;

namespace Moonstorm.Config
{
    /// <summary>
    /// A Configurable <see cref="bool"/> that can be configured using the BepInEx Config System
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <see cref="bool"/></br>
    /// <para>If <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null or empty, this ConfigurableBool is automatically added to RiskOfOptions' <see cref="ModSettingsManager"/></para>
    /// </summary>
    public class ConfigurableBool : ConfigurableVariable<bool>
    {
        /// <summary>
        /// A Configuration for the Risk of Options' <see cref="CheckBoxOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
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

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableBool SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableBool SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableBool SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableBool SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableBool SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }


        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableBool SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableBool SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable{T}.AddOnConfigChanged(ConfigurableVariable{T}.OnConfigChangedDelegate)"/>
        /// </summary>
        public new ConfigurableBool AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="CheckBoxConfig"/>
        /// </summary>
        public ConfigurableBool SetCheckboxConfig(CheckBoxConfig cfg)
        {
            CheckBoxConfig = cfg;
            return this;
        }

        /// <summary>
        /// Chainable method that configures this ConfigurableBool using the specified data. This is normally called automatically by the <see cref="ConfigSystem"/>, but it can be used for early initialization of configs if need be.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        public new ConfigurableBool DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        /// <summary>
        /// When <see cref="DoConfigure"/> is called and <see cref="ConfigurableVariable.ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable
        /// <para>Automatically creates a <see cref="CheckBoxOption"/> for this ConfigurableBool if <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null.</para>
        /// </summary>
        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
            {
                bool separateEntry = ConfigSystem.configFilesWithSeparateRooEntries.Contains(ConfigFile);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(ConfigFile.ConfigFilePath);
                var guid = separateEntry ? ModGUID + "." + fileName : ModGUID;
                var name = separateEntry ? ModName + "." + fileName : ModName;
                CheckBoxOption option = CheckBoxConfig == null ? new CheckBoxOption(ConfigEntry) : new CheckBoxOption(ConfigEntry, CheckBoxConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurableBool"/> with a default value
        /// </summary>
        /// <param name="defaultVal">The default bool value.</param>
        public ConfigurableBool(bool defaultVal) : base(defaultVal)
        {
        }
    }

}