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

namespace MSU.Config
{
    public class ConfiguredFloat : ConfiguredVariable<float>
    {
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

        public new ConfiguredFloat WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredFloat WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredFloat WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredFloat WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredFloat WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredFloat WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredFloat WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public new ConfiguredFloat WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        public new ConfiguredFloat DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfiguredFloat WithSliderType(SliderTypeEnum sliderType)
        {
            SliderType = sliderType;
            return this;
        }

        public ConfiguredFloat WithStepSliderConfig(StepSliderConfig stepSliderConfig)
        {
            StepSliderConfig = stepSliderConfig;
            return this;
        }

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

        public ConfiguredFloat(float value) : base(value) { }

        public enum SliderTypeEnum
        {
            Step,
            Normal
        }
    }
}
