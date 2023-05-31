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
    public class ConfigurableString : ConfigurableVariable<string>
    {
        public InputFieldConfig InputFieldConfig { get; set; }
        public new ConfigurableString SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        public new ConfigurableString SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        public new ConfigurableString SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        public new ConfigurableString SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        public new ConfigurableString SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public ConfigurableString SetInputFieldConfig(InputFieldConfig cfg)
        {
            if (IsConfigured)
                return this;

            InputFieldConfig = cfg;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
            ModSettingsManager.AddOption(new StringInputFieldOption(ConfigEntry, InputFieldConfig), ModGUID, ModName);
        }
        public ConfigurableString(string defaultVal) : base(defaultVal)
        {
        }
    }

}