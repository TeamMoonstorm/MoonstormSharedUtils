using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace Moonstorm.Config
{
    public class ConfigurableKeyBind : ConfigurableVariable<KeyboardShortcut>
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

        public new ConfigurableKeyBind SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        public new ConfigurableKeyBind SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        public new ConfigurableKeyBind SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        public new ConfigurableKeyBind SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        public new ConfigurableKeyBind SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        public new ConfigurableKeyBind SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        public new ConfigurableKeyBind SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        public new ConfigurableKeyBind AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        public new ConfigurableKeyBind DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        public ConfigurableKeyBind SetKeyBindConfig(KeyBindConfig cfg)
        {
            KeyBindConfig = cfg;
            return this;
        }

        public bool IsDown => Value.IsDown();

        public bool IsPressed => Value.IsPressed();

        public bool IsUp() => Value.IsUp();

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(ModGUID.IsNullOrWhiteSpace() || ModName.IsNullOrWhiteSpace()))
            {
                bool separateEntry = ConfigSystem.configFilesWithSeparateRooEntries.Contains(ConfigFile);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(ConfigFile.ConfigFilePath);
                var guid = separateEntry ? ModGUID + "." + fileName : ModGUID;
                var name = separateEntry ? ModName + "." + fileName : ModName;
                KeyBindOption option = KeyBindConfig == null ? new KeyBindOption(ConfigEntry) : new KeyBindOption(ConfigEntry, KeyBindConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        public ConfigurableKeyBind(KeyboardShortcut defaultVal) : base(defaultVal)
        {
        }
    }
}
