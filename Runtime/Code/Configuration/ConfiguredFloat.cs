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
    /// <br>T is float</br>
    /// </summary>
    public class ConfiguredFloat : ConfiguredVariable<float>
    {
        /// <summary>
        /// The type of slider used for this ConfiguredFloat.
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.isConfigured"/> is true</para>
        /// </summary>
        public SliderTypeEnum sliderType
        {
            get => _sliderType;
            set
            {
                if (isConfigured)
                    return;
                _sliderType = value;
            }
        }
        private SliderTypeEnum _sliderType;

        /// <summary>
        /// The StepSliderConfig for this ConfiguredFloat's <see cref="RiskOfOptions.Options.StepSliderOption"/>
        /// <br>Only used if <see cref="sliderType"/> equals to <see cref="SliderTypeEnum.Step"/></br>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.isConfigured"/> is true</para>
        /// </summary>
        public StepSliderConfig stepSliderConfig
        {
            get => _stepSliderConfig;
            set
            {
                if (isConfigured)
                {
                    return;
                }
                _stepSliderConfig = value;
            }
        }
        private StepSliderConfig _stepSliderConfig;

        /// <summary>
        /// The SliderConfig for this ConfiguredFloat's <see cref="RiskOfOptions.Options.SliderOption"/>
        /// <br>Only used if <see cref="sliderType"/> equals to <see cref="SliderTypeEnum.Normal"/></br>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.isConfigured"/> is true</para>
        /// </summary>
        public SliderConfig sliderConfig
        {
            get => _sliderConfig;
            set
            {
                if (isConfigured)
                {
                    return;
                }
                _sliderConfig = value;
            }
        }
        private SliderConfig _sliderConfig;

        /// <inheritdoc cref="ConfiguredVariable{T}.DoConfigure"/>
        public new ConfiguredFloat DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigFile(ConfigFile)"/>
        public new ConfiguredFloat WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithSection(string)"/>
        public new ConfiguredFloat WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(string)"/>
        public new ConfiguredFloat WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(string)"/>
        public new ConfiguredFloat WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(string)"/>
        public new ConfiguredFloat WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(BaseUnityPlugin)"/>
        public new ConfiguredFloat WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(BaseUnityPlugin)"/>
        public new ConfiguredFloat WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        /// <inheritdoc cref="ConfiguredVariable{T}.WithConfigChange(ConfiguredVariable{T}.OnConfigChangedDelegate)"/>
        public new ConfiguredFloat WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="sliderType"/>
        /// </summary>
        public ConfiguredFloat WithSliderType(SliderTypeEnum sliderType)
        {
            this.sliderType = sliderType;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="stepSliderConfig"/>
        /// </summary>
        public ConfiguredFloat WithStepSliderConfig(StepSliderConfig stepSliderConfig)
        {
            this.stepSliderConfig = stepSliderConfig;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="sliderConfig"/>
        /// </summary>
        public ConfiguredFloat WithSliderConfig(SliderConfig sliderConfig)
        {
            this.sliderConfig = sliderConfig;
            return this;
        }
        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(modGUID.IsNullOrWhiteSpace() || modName.IsNullOrWhiteSpace()))
            {
                bool separateEntry = ConfigSystem.ShouldCreateSeparateRiskOfOptionsEntry(configFile);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
                var guid = separateEntry ? modGUID + "." + fileName : modGUID;
                var name = separateEntry ? modName + "." + fileName : modName;
                BaseOption baseOption = null;

                switch (sliderType)
                {
                    case SliderTypeEnum.Normal:
                        baseOption = sliderConfig == null ? new SliderOption(configEntry) : new SliderOption(configEntry, sliderConfig);
                        break;
                    case SliderTypeEnum.Step:
                        baseOption = stepSliderConfig == null ? new StepSliderOption(configEntry) : new StepSliderOption(configEntry, stepSliderConfig);
                        break;
                }
                ModSettingsManager.AddOption(baseOption, guid, name);
            }
        }

        /// <inheritdoc cref="ConfiguredVariable(object)"/>
        public ConfiguredFloat(float value) : base(value) { }

        /// <summary>
        /// Represents the type of slider to use for configuring this ConfiguredFloat
        /// </summary>
        public enum SliderTypeEnum
        {
            /// <summary>
            /// The slider should go in steps increments.
            /// </summary>
            Step,
            /// <summary>
            /// The slider works as normal.
            /// </summary>
            Normal
        }
    }
}
