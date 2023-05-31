using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using BepInEx.Configuration;

namespace Moonstorm.Config
{
    public class ConfigurableFloat : ConfigurableVariable<float>
    {
        public bool UseStepSlider { get; set; }
        public StepSliderConfig StepSliderConfig { get; set; }
        public SliderConfig SliderConfig { get; set; }
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

        public ConfigurableFloat SetUseStepSlider(bool useStepSlider)
        {
            if (IsConfigured)
                return this;

            UseStepSlider = useStepSlider;
            return this;
        }

        public ConfigurableFloat SetStepSliderConfig(StepSliderConfig cfg)
        {
            if (IsConfigured)
                return this;

            StepSliderConfig = cfg;
            return this;
        }

        public ConfigurableFloat SetSliderConfig(SliderConfig cfg)
        {
            if (IsConfigured)
                return this;

            SliderConfig = cfg;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
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
        public ConfigurableFloat(float defaultVal) : base(defaultVal)
        {
        }
    }

}