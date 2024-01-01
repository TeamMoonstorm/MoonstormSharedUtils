using BepInEx;
using BepInEx.Configuration;
using HG.BlendableTypes;
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
    public class ConfiguredKeyBind : ConfiguredVariable<KeyboardShortcut>
    {
        public KeyBindConfig KeyBindConfig
        {
            get => _keyBindConfig;
            set
            {
                if (IsConfigured)
                    return;
                _keyBindConfig = value;
            }
        }
        private KeyBindConfig _keyBindConfig;

        public bool IsDown => Value.IsDown();

        public bool IsPressed => Value.IsPressed();

        public bool IsUp => Value.IsUp();

        public new ConfiguredKeyBind WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        public new ConfiguredKeyBind WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        public new ConfiguredKeyBind WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        public new ConfiguredKeyBind WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        public new ConfiguredKeyBind WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        public new ConfiguredKeyBind WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        public new ConfiguredKeyBind WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        public new ConfiguredKeyBind WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        public new ConfiguredKeyBind DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfiguredKeyBind WithKeyBindConfig(KeyBindConfig config)
        {
            KeyBindConfig = config;
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
                KeyBindOption option = KeyBindConfig == null ? new KeyBindOption(ConfigEntry) : new KeyBindOption(ConfigEntry, KeyBindConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfiguredKeyBind(KeyboardShortcut defaultVal) : base(defaultVal) { }
    }
}
