using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace Moonstorm.Config
{
    public class ConfigurableString : ConfigurableVariable<string>
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

        public new ConfigurableString SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        public new ConfigurableString SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        public new ConfigurableString AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        public ConfigurableString SetInputFieldConfig(InputFieldConfig cfg)
        {
            InputFieldConfig = cfg;
            return this;
        }

        public new ConfigurableString DoConfigure()
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
                var option = InputFieldConfig == null ? new StringInputFieldOption(ConfigEntry) : new StringInputFieldOption(ConfigEntry, InputFieldConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfigurableString(string defaultVal) : base(defaultVal)
        {
        }
    }

}