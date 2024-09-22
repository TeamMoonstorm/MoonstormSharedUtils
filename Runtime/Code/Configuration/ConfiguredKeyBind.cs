using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace MSU.Config
{
    /// <summary>
    /// <inheritdoc cref="ConfiguredVariable{T}"/>
    /// <br>Also contains necesary info for implementing the Config with Risk of Options.</br>
    /// <br>T is <see cref="KeyboardShortcut"/></br>
    /// </summary>
    public class ConfiguredKeyBind : ConfiguredVariable<KeyboardShortcut>
    {
        /// <summary>
        /// An optional KeyBindConfig for this ConfiguredKeyBind's <see cref="RiskOfOptions.Options.KeyBindOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.isConfigured"/> is true</para>
        /// </summary>
        public KeyBindConfig keyBindConfig
        {
            get => _keyBindConfig;
            set
            {
                if (isConfigured)
                {
#if DEBUG
                    LogReadOnly(nameof(keyBindConfig));
#endif
                    return;
                }
                _keyBindConfig = value;
            }
        }
        private KeyBindConfig _keyBindConfig;

        /// <summary>
        /// <inheritdoc cref="KeyboardShortcut.IsDown"/>
        /// </summary>
        public bool isDown => value.IsDown();

        /// <summary>
        /// <inheritdoc cref="KeyboardShortcut.IsPressed"/>
        /// </summary>
        public bool isPressed => value.IsPressed();

        /// <summary>
        /// <inheritdoc cref="KeyboardShortcut.IsUp"/>
        /// </summary>
        public bool isUp => value.IsUp();

        /// <inheritdoc cref="ConfiguredVariable{T}.DoConfigure"/>
        public new ConfiguredKeyBind DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigFile(ConfigFile)"/>
        public new ConfiguredKeyBind WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithSection(string)"/>
        public new ConfiguredKeyBind WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(string)"/>
        public new ConfiguredKeyBind WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(string)"/>
        public new ConfiguredKeyBind WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(string)"/>
        public new ConfiguredKeyBind WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(BaseUnityPlugin)"/>
        public new ConfiguredKeyBind WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(BaseUnityPlugin)"/>
        public new ConfiguredKeyBind WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        /// <inheritdoc cref="ConfiguredVariable{T}.WithConfigChange(ConfiguredVariable{T}.OnConfigChangedDelegate)"/>
        public new ConfiguredKeyBind WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="keyBindConfig"/>
        /// </summary>
        public ConfiguredKeyBind WithKeyBindConfig(KeyBindConfig config)
        {
            keyBindConfig = config;
            return this;
        }
        /// <inheritdoc/>

        protected override void OnConfigured()
        {
            base.OnConfigured();
            if (!(modGUID.IsNullOrWhiteSpace() || modName.IsNullOrWhiteSpace()))
            {
                bool separateEntry = ConfigSystem.ShouldCreateSeparateRiskOfOptionsEntry(configFile);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
                var guid = separateEntry ? modGUID + "." + fileName : modGUID;
                var name = separateEntry ? modName + "." + fileName : modName;
                KeyBindOption option = keyBindConfig == null ? new KeyBindOption(configEntry) : new KeyBindOption(configEntry, keyBindConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <inheritdoc cref="ConfiguredVariable(object)"/>
        public ConfiguredKeyBind(KeyboardShortcut defaultVal) : base(defaultVal) { }
    }
}
