using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
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
    public class ConfiguredEnum<TEnum> : ConfiguredVariable<TEnum> where TEnum : Enum
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

        public new ConfiguredEnum<TEnum> WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredEnum<TEnum> WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredEnum<TEnum> WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredEnum<TEnum> WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredEnum<TEnum> WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredEnum<TEnum> WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredEnum<TEnum> WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public new ConfiguredEnum<TEnum> WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        public new ConfiguredEnum<TEnum> DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfiguredEnum<TEnum> WithChoiceConfig(ChoiceConfig choiceConfig)
        {
            ChoiceConfig = choiceConfig;
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
                var option = ChoiceConfig == null ? new ChoiceOption(ConfigEntry) : new ChoiceOption(ConfigEntry, ChoiceConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfiguredEnum(TEnum defaultVal) : base(defaultVal) { }
    }
}