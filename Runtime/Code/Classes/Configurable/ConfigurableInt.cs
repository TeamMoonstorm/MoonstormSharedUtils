using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;

namespace Moonstorm.Config
{
    /// <summary>
    /// A Configurable int that can be configured using the BepInEx Config System
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <see cref="int"/></br>
    /// <para>If <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null or empty, this ConfigurableInt is automatically added to RiskOfOptions' <see cref="ModSettingsManager"/></para>
    /// </summary>
    public class ConfigurableInt : ConfigurableVariable<int>
    {
        /// <summary>
        /// A Configuration for the RiskOfOptions' <see cref="IntSliderOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
        public IntSliderConfig SliderConfig
        {
            get => _sliderConfig;
            set
            {
                if (IsConfigured)
                    return;
                _sliderConfig = value;
            }
        }
        private IntSliderConfig _sliderConfig;

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableInt SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableInt SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableInt SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableInt SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableInt SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableInt SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableInt SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        [Obsolete("Method is wrongly named, Use AddOnConfigChanged instead")]
        public new ConfigurableInt SetOnConfigChanged(OnConfigChangedDelegate onConfigChanged) => AddOnConfigChanged(onConfigChanged);

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable{T}.AddOnConfigChanged(ConfigurableVariable{T}.OnConfigChangedDelegate)"/>
        /// </summary>
        public new ConfigurableInt AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="SliderConfig"/>
        /// </summary>
        public ConfigurableInt SetSliderConfig(IntSliderConfig cfg)
        {
            SliderConfig = cfg;
            return this;
        }

        /// <summary>
        /// Chainable method that configures this ConfigurableInt using the specified data. This is normally called automatically by the <see cref="ConfigSystem"/>, but it can be used for early initialization of configs if need be.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        public new ConfigurableInt DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        /// <summary>
        /// When <see cref="DoConfigure"/> is called and <see cref="ConfigurableVariable.ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable.
        /// <para>Automatically creates an <see cref="IntSliderOption"/> for this ConfigurableInt if <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null.</para>
        /// </summary>
        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
            {
                var option = SliderConfig == null ? new IntSliderOption(ConfigEntry) : new IntSliderOption(ConfigEntry, SliderConfig);
                ModSettingsManager.AddOption(option, ModGUID, ModName);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurableInt"/> with a default value
        /// </summary>
        /// <param name="defaultVal">The default int value.</param>
        public ConfigurableInt(int defaultVal) : base(defaultVal)
        {
        }
    }

}