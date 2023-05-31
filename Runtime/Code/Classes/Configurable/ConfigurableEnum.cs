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
    public class ConfigurableEnum<TEnum> : ConfigurableVariable<TEnum> where TEnum : Enum
    {
        public ChoiceConfig ChoiceConfig
        {
            get => _choiceConfig;
            set
            {
                if (IsConfigured)
                    return;
                _choiceConfig = value;
            }
        }
        private ChoiceConfig _choiceConfig;

        public new ConfigurableEnum<TEnum> SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        public new ConfigurableEnum<TEnum> SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        public new ConfigurableEnum<TEnum> SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        public new ConfigurableEnum<TEnum> SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        public new ConfigurableEnum<TEnum> SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public new ConfigurableEnum<TEnum> SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        public new ConfigurableEnum<TEnum> SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        public ConfigurableEnum<TEnum> SetChoiceConfig(ChoiceConfig cfg)
        {
            ChoiceConfig = cfg;
            return this;
        }

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
            {
                var option = ChoiceConfig == null ? new ChoiceOption(ConfigEntry) : new ChoiceOption(ConfigEntry, ChoiceConfig);
                ModSettingsManager.AddOption(option, ModGUID, ModName);
            }
        }
        public ConfigurableEnum(TEnum defaultVal) : base(defaultVal)
        {
        }
    }

}