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
using UnityEngine;

namespace MSU.Config
{
    public class ConfiguredColor : ConfiguredVariable<Color>
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

        public new ConfiguredColor WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredColor WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredColor WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredColor WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredColor WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredColor WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredColor WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public new ConfiguredColor WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        public new ConfiguredColor DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfiguredColor WithColorOptionConfig(ColorOptionConfig config)
        {
            ColorConfig = config;
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
                var option = ColorConfig == null ? new ColorOption(ConfigEntry) : new ColorOption(ConfigEntry, ColorConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfiguredColor(Color defaultColor) : base(defaultColor) { }
    }
}
