using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;

namespace Moonstorm.Config
{
    public class ConfigurableBool : ConfigurableVariable<bool>
    {
        public CheckBoxConfig CheckBoxConfig
        {
            get => _checkBoxConfig;
            set
            {
                if (IsConfigured)
                    return;
                _checkBoxConfig = value;
            }
        }
        private CheckBoxConfig _checkBoxConfig;

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

        public new ConfigurableBool SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }


        public new ConfigurableBool SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        public new ConfigurableBool SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public new ConfigurableBool AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        public ConfigurableBool SetCheckboxConfig(CheckBoxConfig cfg)
        {
            CheckBoxConfig = cfg;
            return this;
        }

        public new ConfigurableBool DoConfigure()
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
                CheckBoxOption option = CheckBoxConfig == null ? new CheckBoxOption(ConfigEntry) : new CheckBoxOption(ConfigEntry, CheckBoxConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfigurableBool(bool defaultVal) : base(defaultVal)
        {
        }
    }

}