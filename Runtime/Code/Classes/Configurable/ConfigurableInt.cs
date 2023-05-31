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
    public class ConfigurableInt : ConfigurableVariable<int>
    {
        public IntSliderConfig SliderConfig { get; set; }
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

        public ConfigurableInt SetSliderConfig(IntSliderConfig cfg)
        {
            if (IsConfigured)
                return this;

            SliderConfig = cfg;
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

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
                ModSettingsManager.AddOption(new IntSliderOption(ConfigEntry, SliderConfig), ModGUID, ModName);
        }
        public ConfigurableInt(int defaultVal) : base(defaultVal)
        {
        }
    }

}