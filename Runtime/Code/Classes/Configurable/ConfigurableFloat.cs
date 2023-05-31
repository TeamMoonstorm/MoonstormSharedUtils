using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using BepInEx.Configuration;
using BepInEx;

namespace Moonstorm.Config
{
    public class ConfigurableFloat : ConfigurableVariable<float>
    {
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

        public new ConfigurableFloat SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        public new ConfigurableFloat SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        public new ConfigurableFloat SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        public new ConfigurableFloat SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        public new ConfigurableFloat SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public new ConfigurableFloat SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        public new ConfigurableFloat SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        public ConfigurableFloat SetUseStepSlider(bool useStepSlider)
        {
            UseStepSlider = useStepSlider;
            return this;
        }

        public ConfigurableFloat SetStepSliderConfig(StepSliderConfig cfg)
        {
            StepSliderConfig = cfg;
            return this;
        }

        public ConfigurableFloat SetSliderConfig(SliderConfig cfg)
        {
            SliderConfig = cfg;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
            {
                BaseOption baseOption = null;
                if(UseStepSlider)
                {
                    baseOption = new StepSliderOption(ConfigEntry, StepSliderConfig);
                }
                else
                {
                    baseOption = new SliderOption(ConfigEntry, SliderConfig);
                }
                ModSettingsManager.AddOption(baseOption, ModGUID, ModName);
            }
        }
        public ConfigurableFloat(float defaultVal) : base(defaultVal)
        {
        }
    }

}