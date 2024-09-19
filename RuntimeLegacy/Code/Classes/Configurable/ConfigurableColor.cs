using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

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


        public new ConfigurableColor AddOnConfigChanged(OnConfigChangedDelegate onConfigChangedDelegate)
        {
            base.AddOnConfigChanged(onConfigChangedDelegate);
            return this;
        }

        public ConfigurableColor SetColorConfig(ColorOptionConfig cfg)
        {
            ColorConfig = cfg;
            return this;
        }

        public new ConfigurableColor DoConfigure()
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
                var option = ColorConfig == null ? new ColorOption(ConfigEntry) : new ColorOption(ConfigEntry, ColorConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfigurableColor(Color defaultVal) : base(defaultVal)
        {
        }
    }

}