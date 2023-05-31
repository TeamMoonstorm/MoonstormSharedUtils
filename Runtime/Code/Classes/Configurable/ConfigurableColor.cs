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
using UnityEditor;
using BepInEx;

namespace Moonstorm.Config
{
    public class ConfigurableColor : ConfigurableVariable<Color>
    {
        public ColorOptionConfig ColorConfig
        {
            get => _colorConfig;
            set
            {
                if (IsConfigured)
                    return;
                _colorConfig = value;
            }
        }
        private ColorOptionConfig _colorConfig;

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

        public new ConfigurableColor SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        public new ConfigurableColor SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        public ConfigurableColor SetColorConfig(ColorOptionConfig cfg)
        {
            ColorConfig = cfg;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
                ModSettingsManager.AddOption(new ColorOption(ConfigEntry, ColorConfig), ModGUID, ModName);
        }
        public ConfigurableColor(Color defaultVal) : base(defaultVal)
        {
        }
    }

}