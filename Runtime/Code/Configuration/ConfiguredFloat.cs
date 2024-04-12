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
using UnityEditor;

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
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.IsConfigured"/> is true</para>
        /// </summary>
        public SliderTypeEnum SliderType
        {
            get => _sliderType;
            set
            {
                if (IsConfigured)
                    return;
                _sliderType = value;
            }
        }
        private SliderTypeEnum _sliderType;

        /// <summary>
        /// The StepSliderConfig for this ConfiguredFloat's <see cref="RiskOfOptions.Options.StepSliderOption"/>
        /// <br>Only used if <see cref="SliderType"/> equals to <see cref="SliderTypeEnum.Step"/></br>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.IsConfigured"/> is true</para>
        /// </summary>
        public StepSliderConfig StepSliderConfig
        {
            get => _stepSliderConfig;
            set
            {
                if (IsConfigured)
                {
                    return;
                }
                _stepSliderConfig = value;
            }
        }
        private StepSliderConfig _stepSliderConfig;

        /// <summary>
        /// The SliderConfig for this ConfiguredFloat's <see cref="RiskOfOptions.Options.SliderOption"/>
        /// <br>Only used if <see cref="SliderType"/> equals to <see cref="SliderTypeEnum.Normal"/></br>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.IsConfigured"/> is true</para>
        /// </summary>
        public SliderConfig SliderConfig
        {
            get => _sliderConfig;
            set
            {
                if(IsConfigured)
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

        ///<inheritdoc cref="ConfiguredVariable.WithSection(ConfigFile)"/>
        public new ConfiguredFloat WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(ConfigFile)"/>
        public new ConfiguredFloat WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(ConfigFile)"/>
        public new ConfiguredFloat WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(ConfigFile)"/>
        public new ConfiguredFloat WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(ConfigFile)"/>
        public new ConfiguredFloat WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(ConfigFile)"/>
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
        /// Chainable method for setting <see cref="SliderType"/>
        /// </summary>
        public ConfiguredFloat WithSliderType(SliderTypeEnum sliderType)
        {
            SliderType = sliderType;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="StepSliderConfig"/>
        /// </summary>
        public ConfiguredFloat WithStepSliderConfig(StepSliderConfig stepSliderConfig)
        {
            StepSliderConfig = stepSliderConfig;
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="SliderConfig"/>
        /// </summary>
        public ConfiguredFloat WithSliderConfig(SliderConfig sliderConfig)
        {
            SliderConfig = sliderConfig;
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
                BaseOption baseOption = null;
                
                switch(SliderType)
                {
                    case SliderTypeEnum.Normal:
                        baseOption = SliderConfig == null ? new SliderOption(ConfigEntry) : new SliderOption(ConfigEntry, SliderConfig);
                        break;
                    case SliderTypeEnum.Step:
                        baseOption = StepSliderConfig == null ? new StepSliderOption(ConfigEntry) : new StepSliderOption(ConfigEntry, StepSliderConfig);
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
