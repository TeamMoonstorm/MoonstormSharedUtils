using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using BepInEx.Configuration;
using UnityEngine;

namespace Moonstorm.Config
{
    public class ConfigurableColor : ConfigurableVariable<Color>
    {
        public ColorOptionConfig ColorConfig { get; set; }
        public new ConfigurableColor SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        public new ConfigurableColor SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        public new ConfigurableColor SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        public new ConfigurableColor SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        public new ConfigurableColor SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public ConfigurableColor SetCheckboxConfig(ColorOptionConfig cfg)
        {
            if (IsConfigured)
                return this;

            ColorConfig = cfg;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
            ModSettingsManager.AddOption(new ColorOption(ConfigEntry, ColorConfig), ModGUID, ModName);
        }
        public ConfigurableColor(Color defaultVal) : base(defaultVal)
        {
        }
    }

}