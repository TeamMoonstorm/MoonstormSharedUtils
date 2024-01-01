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
    public class ConfiguredInt : ConfiguredVariable<int>
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

        public new ConfiguredInt WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredInt WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredInt WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredInt WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredInt WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredInt WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredInt WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public new ConfiguredInt WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        public new ConfiguredInt DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfiguredInt WithIntSliderConfig(IntSliderConfig config)
        {
            SliderConfig = config;
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
                var option = SliderConfig == null ? new IntSliderOption(ConfigEntry) : new IntSliderOption(ConfigEntry, SliderConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfiguredInt(int value) : base(value) { }
    }
}