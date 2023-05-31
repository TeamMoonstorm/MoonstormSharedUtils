using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;

namespace Moonstorm.Config
{
    /// <summary>
    /// A Configurable Enum that can be configured using the BepInEx Config System
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <typeparamref name="TEnum"/></br>
    /// <para>If <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null or empty, this ConfigurableEnum is automatically added to RiskOfOptions' <see cref="ModSettingsManager"/></para>
    /// </summary>
    /// <typeparam name="TEnum">The type of Enum to configure</typeparam>
    public class ConfigurableEnum<TEnum> : ConfigurableVariable<TEnum> where TEnum : Enum
    {

        /// <summary>
        /// A Configuration for the Risk of Options' <see cref="ChoiceOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
        public ChoiceConfig ChoiceConfig
        {
            get => _choiceConfig;
            set
            {
                if (IsConfigured)
                    return;
                _choiceConfig = value;
            }
        }
        private ChoiceConfig _choiceConfig;

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable{T}.AddOnConfigChanged(ConfigurableVariable{T}.OnConfigChangedDelegate)"/>
        /// </summary>
        public new ConfigurableEnum<TEnum> SetOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ChoiceConfig"/>
        /// </summary>
        public ConfigurableEnum<TEnum> SetChoiceConfig(ChoiceConfig cfg)
        {
            ChoiceConfig = cfg;
            return this;
        }

        /// <summary>
        /// Chainable method that configures this ConfigurableEnum using the specified data. This is normally called automatically by the <see cref="ConfigSystem"/>, but it can be used for early initialization of configs if need be.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        public new ConfigurableEnum<TEnum> DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        /// <summary>
        /// When <see cref="DoConfigure"/> is called and <see cref="ConfigurableVariable.ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable
        /// <para>Automatically creates a <see cref="ChoiceOption"/> for this ConfigurableEnum if <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null.</para>
        /// </summary>
        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
            {
                var option = ChoiceConfig == null ? new ChoiceOption(ConfigEntry) : new ChoiceOption(ConfigEntry, ChoiceConfig);
                ModSettingsManager.AddOption(option, ModGUID, ModName);
            }
        }

        /// <summary>
        /// Creates a new isntance of <see cref="ConfigurableEnum{TEnum}"/> with a default value
        /// </summary>
        /// <param name="defaultVal">The default Enum value</param>
        public ConfigurableEnum(TEnum defaultVal) : base(defaultVal)
        {
        }
    }
}