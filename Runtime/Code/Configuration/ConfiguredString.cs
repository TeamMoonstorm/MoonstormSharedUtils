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
    public class ConfiguredString : ConfiguredVariable<string>
    {
        public InputFieldConfig InputFieldConfig
        {
            get => _inputFieldConfig;
            set
            {
                if (IsConfigured)
                    return;
                _inputFieldConfig = value;
            }
        }
        private InputFieldConfig _inputFieldConfig;

        public new ConfiguredString WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredString WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredString WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredString WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredString WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredString WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredString WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public new ConfiguredString WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        public new ConfiguredString DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfiguredString WithInputFieldConfig(InputFieldConfig config)
        {
            InputFieldConfig = config;
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
                var option = InputFieldConfig == null ? new StringInputFieldOption(ConfigEntry) : new StringInputFieldOption(ConfigEntry, InputFieldConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfiguredString(string defaultVal) : base(defaultVal) { }
    }
}
