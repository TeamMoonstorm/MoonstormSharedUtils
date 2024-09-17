using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace Moonstorm.Config
{
    public class ConfigurableInt : ConfigurableVariable<int>
    {
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

        public new ConfigurableInt SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        public new ConfigurableInt SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        public new ConfigurableInt SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        public new ConfigurableInt SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        public new ConfigurableInt SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public new ConfigurableInt SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        public new ConfigurableInt SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        public new ConfigurableInt AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        public ConfigurableInt SetSliderConfig(IntSliderConfig cfg)
        {
            SliderConfig = cfg;
            return this;
        }

        public new ConfigurableInt DoConfigure()
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
                var option = SliderConfig == null ? new IntSliderOption(ConfigEntry) : new IntSliderOption(ConfigEntry, SliderConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfigurableInt(int defaultVal) : base(defaultVal)
        {
        }
    }

}