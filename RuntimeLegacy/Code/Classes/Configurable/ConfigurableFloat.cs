using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

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

        public new ConfigurableFloat AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
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

        public new ConfigurableFloat DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

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

        public ConfigurableFloat(float defaultVal) : base(defaultVal)
        {
        }
    }

}