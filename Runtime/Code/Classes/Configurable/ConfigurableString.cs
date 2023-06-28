using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;

namespace Moonstorm.Config
{
    /// <summary>
    /// A Configurable string that can be configured using the BepInEx Config System
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <see cref="string"/></br>
    /// <para>If <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null or empty, this ConfigurableString is automatically added to RiskOfOptions' <see cref="ModSettingsManager"/></para>
    /// </summary>
    public class ConfigurableString : ConfigurableVariable<string>
    {
        /// <summary>
        /// A Configuration for the RiskOfOptions' <see cref="StringInputFieldOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
        public InputFieldConfig InputFieldConfig
        {
            get => _inputFieldConfig;
            set
            {
                if (IsConfigured)
                    return;
                _inputFieldConfig = value;
            }
        }
        private InputFieldConfig _inputFieldConfig;

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableString SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableString SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableString SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableString SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableString SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableString SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableString SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        [Obsolete("Method is wrongly named, Use AddOnConfigChanged instead")]
        public new ConfigurableString SetOnConfigChanged(OnConfigChangedDelegate onConfigChanged) => AddOnConfigChanged(onConfigChanged);

        public new ConfigurableString AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="InputFieldConfig"/>
        /// </summary>
        public ConfigurableString SetInputFieldConfig(InputFieldConfig cfg)
        {
            InputFieldConfig = cfg;
            return this;
        }

        /// <summary>
        /// Chainable method that configures this ConfigurableString using the specified data. This is normally called automatically by the <see cref="ConfigSystem"/>, but it can be used for early initialization of configs if need be.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        public new ConfigurableString DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        /// <summary>
        /// When <see cref="DoConfigure"/> is called and <see cref="ConfigurableVariable.ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable.
        /// <para>Automatically creates an <see cref="StringInputFieldOption"/> for this ConfigurableString if <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null.</para>
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
                var option = InputFieldConfig == null ? new StringInputFieldOption(ConfigEntry) : new StringInputFieldOption(ConfigEntry, InputFieldConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurableString"/> with a default value
        /// </summary>
        /// <param name="defaultVal">The default string value.</param>
        public ConfigurableString(string defaultVal) : base(defaultVal)
        {
        }
    }

}