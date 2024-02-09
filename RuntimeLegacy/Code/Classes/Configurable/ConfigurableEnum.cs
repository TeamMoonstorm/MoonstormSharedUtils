using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;

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

        public new ConfigurableEnum<TEnum> AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        public ConfigurableEnum<TEnum> SetChoiceConfig(ChoiceConfig cfg)
        {
            ChoiceConfig = cfg;
            return this;
        }

        public new ConfigurableEnum<TEnum> DoConfigure()
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
                var option = ChoiceConfig == null ? new ChoiceOption(ConfigEntry) : new ChoiceOption(ConfigEntry, ChoiceConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfigurableEnum(TEnum defaultVal) : base(defaultVal)
        {
        }
    }
}