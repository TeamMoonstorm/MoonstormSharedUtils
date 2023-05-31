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
    public class ConfigurableBool : ConfigurableVariable<bool>
    {
        public CheckBoxConfig CheckBoxConfig { get; set; }
        public new ConfigurableBool SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        public new ConfigurableBool SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        public new ConfigurableBool SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        public new ConfigurableBool SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        public new ConfigurableBool SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public ConfigurableBool SetCheckboxConfig(CheckBoxConfig cfg)
        {
            if (IsConfigured)
                return this;

            CheckBoxConfig = cfg;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
            ModSettingsManager.AddOption(new CheckBoxOption(ConfigEntry, CheckBoxConfig), ModGUID, ModName);
        }
        public ConfigurableBool(bool defaultVal) : base(defaultVal)
        {
        }
    }

}