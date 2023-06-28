using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;

namespace Moonstorm.Config
{
    /// <summary>
    /// A Configurable float that can be configured using the BepInEx Config System
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <see cref="float"/></br>
    /// <para>If <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null or empty, this ConfigurableFloat is automatically added to RiskOfOptions' <see cref="ModSettingsManager"/></para>
    /// </summary>
    public class ConfigurableFloat : ConfigurableVariable<float>
    {

        /// <summary>
        /// Wether the ConfigurableFloat uses a <see cref="SliderOption"/> or a <see cref="StepSliderOption"/>
        /// <br>If true, a <see cref="StepSliderOption"/> is created using <see cref="StepSliderConfig"/>, otherwise a <see cref="SliderOption"/> is created using <see cref="SliderConfig"/></br>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
        public bool UseStepSlider
        {
            get => _useStepSlider;
            set
            {
                if (IsConfigured)
                    return;
                _useStepSlider = value;
            }
        }
        private bool _useStepSlider;

        /// <summary>
        /// A Configuration for the RiskOfOptions' <see cref="SliderOption"/>
        /// <br>Used if <see cref="UseStepSlider"/> is true</br>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
        public StepSliderConfig StepSliderConfig
        {
            get => _stepSliderConfig;
            set
            {
                if (IsConfigured)
                    return;
                _stepSliderConfig = value;
            }
        }

        /// <summary>
        /// A Configuration for the RiskOfOptions' <see cref="StepSliderOption"/>
        /// <br>Used if <see cref="UseStepSlider"/> is false</br>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
        private StepSliderConfig _stepSliderConfig;
        public SliderConfig SliderConfig
        {
            get => _sliderConfig;
            set
            {
                if (IsConfigured)
                    return;
                _sliderConfig = value;
            }
        }
        private SliderConfig _sliderConfig;

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableFloat SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableFloat SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableFloat SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableFloat SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableFloat SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableFloat SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableFloat SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        [Obsolete("Method is wrongly named, Use AddOnConfigChanged instead")]
        public new ConfigurableFloat SetOnConfigChanged(OnConfigChangedDelegate onConfigChanged) => AddOnConfigChanged(onConfigChanged);

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable{T}.AddOnConfigChanged(ConfigurableVariable{T}.OnConfigChangedDelegate)"/>
        /// </summary>
        public new ConfigurableFloat AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="UseStepSlider"/>
        /// </summary>
        public ConfigurableFloat SetUseStepSlider(bool useStepSlider)
        {
            UseStepSlider = useStepSlider;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="StepSliderConfig"/>
        /// </summary>
        public ConfigurableFloat SetStepSliderConfig(StepSliderConfig cfg)
        {
            StepSliderConfig = cfg;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="SliderConfig"/>
        /// </summary>
        public ConfigurableFloat SetSliderConfig(SliderConfig cfg)
        {
            SliderConfig = cfg;
            return this;
        }

        /// <summary>
        /// Chainable method that configures this ConfigurableFloat using the specified data. This is normally called automatically by the <see cref="ConfigSystem"/>, but it can be used for early initialization of configs if need be.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        public new ConfigurableFloat DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        /// <summary>
        /// When <see cref="DoConfigure"/> is called and <see cref="ConfigurableVariable.ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable.
        /// <para>Automatically creates a <see cref="StepSliderOption"/> or <see cref="SliderOption"/> for this ConfigurableBool if <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null. The type of option is chosen from the value of <see cref="UseStepSlider"/></para>
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
                BaseOption baseOption = null;
                if (UseStepSlider)
                {
                    baseOption = StepSliderConfig == null ? new StepSliderOption(ConfigEntry) : new StepSliderOption(ConfigEntry, StepSliderConfig);
                }
                else
                {
                    baseOption = SliderConfig == null ? new SliderOption(ConfigEntry) : new SliderOption(ConfigEntry, SliderConfig);
                }
                ModSettingsManager.AddOption(baseOption, guid, name);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurableFloat"/> with a default value
        /// </summary>
        /// <param name="defaultVal">The default float value.</param>
        public ConfigurableFloat(float defaultVal) : base(defaultVal)
        {
        }
    }

}