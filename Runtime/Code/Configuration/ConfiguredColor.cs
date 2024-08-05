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
    /// <summary>
    /// <inheritdoc cref="ConfiguredVariable{T}"/>
    /// <br>Also contains necesary info for implementing the Config with Risk of Options.</br>
    /// <br>T is Color</br>
    /// </summary>
    public class ConfiguredColor : ConfiguredVariable<Color>
    {
        /// <summary>
        /// An optional ColorOptionConfig for this ConfiguredColor's <see cref="RiskOfOptions.Options.ColorOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.IsConfigured"/> is true</para>
        /// </summary>
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

        /// <inheritdoc cref="ConfiguredVariable{T}.DoConfigure"/>
        public new ConfiguredColor DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigFile(ConfigFile)"/>
        public new ConfiguredColor WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithSection(string)"/>
        public new ConfiguredColor WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(string)"/>
        public new ConfiguredColor WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(string)"/>
        public new ConfiguredColor WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(string)"/>
        public new ConfiguredColor WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(BaseUnityPlugin)"/>
        public new ConfiguredColor WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(BaseUnityPlugin)"/>
        public new ConfiguredColor WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        /// <inheritdoc cref="ConfiguredVariable{T}.WithConfigChange(ConfiguredVariable{T}.OnConfigChangedDelegate)"/>
        public new ConfiguredColor WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        /// <summary>
        /// Chainable method fors setting <see cref="ColorConfig"/>
        /// </summary>
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

        /// <inheritdoc cref="ConfiguredVariable.ConfiguredVariable(object)"/>
        public ConfiguredColor(Color defaultColor) : base(defaultColor) { }
    }
}
