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
    public class ConfiguredBool : ConfiguredVariable<bool>
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

        public new ConfiguredBool WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredBool WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredBool WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredBool WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredBool    WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredBool WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredBool WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public new ConfiguredBool WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        public new ConfiguredBool DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfiguredBool WithCheckBoxConfig(CheckBoxConfig config)
        {
            CheckBoxConfig = config;
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
                CheckBoxOption option = CheckBoxConfig == null ? new CheckBoxOption(ConfigEntry) : new CheckBoxOption(ConfigEntry, CheckBoxConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfiguredBool(bool defaultVal) : base(defaultVal) { }
    }
}