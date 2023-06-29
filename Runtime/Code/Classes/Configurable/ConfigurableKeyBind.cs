using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace Moonstorm.Config
{
    /// <summary>
    /// A Configurable <see cref="KeyboardShortcut"/> that can be configured using the BepInEx Config System
    /// <br>Contains an implicit operator for casting the ConfigEntry's value into <see cref="KeyboardShortcut"/></br>
    /// <para>If <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null or empty, this ConfigurableKeyBind is automatically added to RiskOfOptions' <see cref="ModSettingsManager"/></para>
    /// </summary>
    public class ConfigurableKeyBind : ConfigurableVariable<KeyboardShortcut>
    {
        /// <summary>
        /// A Configuration for the Risk of Options' <see cref="KeyBindOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfigurableVariable.IsConfigured"/> is true</para>
        /// </summary>
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

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetSection(string)"/>
        /// </summary>
        public new ConfigurableKeyBind SetSection(string section)
        {
            base.SetSection(section);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetKey(string)"/>
        /// </summary>
        public new ConfigurableKeyBind SetKey(string key)
        {
            base.SetKey(key);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetDescription(string)"/>
        /// </summary>
        public new ConfigurableKeyBind SetDescription(string description)
        {
            base.SetDescription(description);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetIdentifier(string)"/>
        /// </summary>
        public new ConfigurableKeyBind SetIdentifier(string identifier)
        {
            base.SetIdentifier(identifier);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetConfigFile(ConfigFile)"/>
        /// </summary>
        public new ConfigurableKeyBind SetConfigFile(ConfigFile file)
        {
            base.SetConfigFile(file);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModGUID(string)"/>
        /// </summary>
        public new ConfigurableKeyBind SetModGUID(string modGUID)
        {
            base.SetModGUID(modGUID);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable.SetModName(string)"/>
        /// </summary>
        public new ConfigurableKeyBind SetModName(string modName)
        {
            base.SetModName(modName);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable{T}.AddOnConfigChanged(ConfigurableVariable{T}.OnConfigChangedDelegate)"/>
        /// </summary>
        public new ConfigurableKeyBind AddOnConfigChanged(OnConfigChangedDelegate onConfigChanged)
        {
            base.AddOnConfigChanged(onConfigChanged);
            return this;
        }

        /// <summary>
        /// <inheritdoc cref="ConfigurableVariable{T}.DoConfigure"/>
        /// </summary>
        public new ConfigurableKeyBind DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="KeyBindConfig"/>
        /// </summary>
        public ConfigurableKeyBind SetKeyBindConfig(KeyBindConfig cfg)
        {
            KeyBindConfig = cfg;
            return this;
        }

        /// <summary>
        /// Checks if the Key combination for this ConfigurableKeyBind was just pressed (Input.GetKeyDown)
        /// </summary>
        public bool IsDown => Value.IsDown();

        /// <summary>
        /// Checks if the Key combination for this ConfigurableKeyBind is currently held down (Input.GetKey)
        /// </summary>
        public bool IsPressed => Value.IsPressed();

        /// <summary>
        /// Checks if the Key combination for this keybind was just lifted (Input.GetKeyUp)
        /// </summary>
        public bool IsUp() => Value.IsUp();

        /// <summary>
        /// When <see cref="DoConfigure"/> is called and <see cref="ConfigurableVariable.ConfigEntry"/> is bound, this method gets called. use it to finalize any initialization of the ConfigurableVariable.
        /// <para>Automatically creates a <see cref="KeyBindOption"/> for this ConfigurableKeyBind if <see cref="ConfigurableVariable.ModGUID"/> and <see cref="ConfigurableVariable.ModName"/> are not null.</para>
        /// </summary>
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

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurableKeyBind"/> with a default value
        /// </summary>
        /// <param name="defaultVal">The default KeyboardShortcut value.</param>
        public ConfigurableKeyBind(KeyboardShortcut defaultVal) : base(defaultVal)
        {
        }
    }
}
