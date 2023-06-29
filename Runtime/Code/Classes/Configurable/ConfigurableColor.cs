using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using UnityEngine;

namespace Moonstorm.Config
{
    /// <summary>
    /// A Configurable <see cref="Color"/> that can be configured using the BepInEx Config System
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <see cref="Color"/></br>
    /// <para>If <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null or empty, this ConfigurableColor is automatically added to RiskOfOptions' <see cref="ModSettingsManager"/></para>
    /// </summary>
    public class ConfigurableColor : ConfigurableVariable<Color>
    {

        /// <summary>
        /// A Configuration for the Risk of Options' <see cref="ColorOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
        public ColorOptionConfig ColorConfig
        {
            get => _colorConfig;
            set
            {
                if (IsConfigured)
                    return;
                _colorConfig = value;
            }
        }
        private ColorOptionConfig _colorConfig;

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableColor SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableColor SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableColor SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableColor SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableColor SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableColor SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableColor SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        [Obsolete("Method is wrongly named, Use AddOnConfigChanged instead")]
        public new ConfigurableColor SetOnConfigChanged(OnConfigChangedDelegate onConfigChanged) => AddOnConfigChanged(onConfigChanged);

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable{T}.AddOnConfigChanged(ConfigurableVariable{T}.OnConfigChangedDelegate)"/>
        /// </summary>
        public new ConfigurableColor AddOnConfigChanged(OnConfigChangedDelegate onConfigChangedDelegate)
        {
            base.AddOnConfigChanged(onConfigChangedDelegate);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ColorConfig"/>
        /// </summary>
        public ConfigurableColor SetColorConfig(ColorOptionConfig cfg)
        {
            ColorConfig = cfg;
            return this;
        }

        /// <summary>
        /// Chainable method that configures this ConfigurableColor using the specified data. This is normally called automatically by the <see cref="ConfigSystem"/>, but it can be used for early initialization of configs if need be.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        public new ConfigurableColor DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        /// <summary>
        /// When <see cref="DoConfigure"/> is called and <see cref="ConfigurableVariable.ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable
        /// <para>Automatically creates a <see cref="ColorOption"/> for this ConfigurableBool if <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null.</para>
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
                var option = ColorConfig == null ? new ColorOption(ConfigEntry) : new ColorOption(ConfigEntry, ColorConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurableColor"/> with a default value
        /// </summary>
        /// <param name="defaultVal">The default Color value.</param>
        public ConfigurableColor(Color defaultVal) : base(defaultVal)
        {
        }
    }

}