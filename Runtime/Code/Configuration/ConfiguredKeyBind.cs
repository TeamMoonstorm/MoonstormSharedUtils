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
    /// <summary>
    /// <inheritdoc cref="ConfiguredVariable{T}"/>
    /// <br>Also contains necesary info for implementing the Config with Risk of Options.</br>
    /// <br>T is <see cref="KeyboardShortcut"/></br>
    /// </summary>
    public class ConfiguredKeyBind : ConfiguredVariable<KeyboardShortcut>
    {
        /// <summary>
        /// An optional KeyBindConfig for this ConfiguredKeyBind's <see cref="RiskOfOptions.Options.KeyBindOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.IsConfigured"/> is true</para>
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
        /// <inheritdoc cref="KeyboardShortcut.IsDown"/>
        /// </summary>
        public bool IsDown => Value.IsDown();

        /// <summary>
        /// <inheritdoc cref="KeyboardShortcut.IsPressed"/>
        /// </summary>
        public bool IsPressed => Value.IsPressed();

        /// <summary>
        /// <inheritdoc cref="KeyboardShortcut.IsUp"/>
        /// </summary>
        public bool IsUp => Value.IsUp();

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

        ///<inheritdoc cref="ConfiguredVariable.WithSection(ConfigFile)"/>
        public new ConfiguredKeyBind WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(ConfigFile)"/>
        public new ConfiguredKeyBind WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(ConfigFile)"/>
        public new ConfiguredKeyBind WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(ConfigFile)"/>
        public new ConfiguredKeyBind WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(ConfigFile)"/>
        public new ConfiguredKeyBind WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(ConfigFile)"/>
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
        /// Chainable method for setting <see cref="KeyBindConfig"/>
        /// </summary>
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

        /// <inheritdoc cref="ConfiguredVariable(object)"/>
        public ConfiguredKeyBind(KeyboardShortcut defaultVal) : base(defaultVal) { }
    }
}
