using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
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
    /// <br>T is <typeparamref name="TEnum"/></br>
    /// </summary>
    /// <typeparam name="TEnum">The type of enum this ConfiguredEnum uses</typeparam>
    public class ConfiguredEnum<TEnum> : ConfiguredVariable<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// An optional ChoiceConfig for this ConfiguredEnum's <see cref="RiskOfOptions.Options.ChoiceOption"/>
        /// <para>Becomes ReadOnly if <see cref="ConfiguredVariable.IsConfigured"/> is true</para>
        /// </summary>
        public ChoiceConfig ChoiceConfig
        {
            get => _choiceConfig;
            set
            {
                if (IsConfigured)
                    return;
                _choiceConfig = value;
            }
        }
        private ChoiceConfig _choiceConfig;

        /// <inheritdoc cref="ConfiguredVariable{T}.DoConfigure"/>
        public new ConfiguredEnum<TEnum> DoConfigure()
        {
            base.DoConfigure();
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigFile(ConfigFile)"/>
        public new ConfiguredEnum<TEnum> WithConfigFile(ConfigFile configFile)
        {
            base.WithConfigFile(configFile);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithSection(string)"/>
        public new ConfiguredEnum<TEnum> WithSection(string section)
        {
            base.WithSection(section);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithKey(string)"/>
        public new ConfiguredEnum<TEnum> WithKey(string key)
        {
            base.WithKey(key);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithDescription(string)"/>
        public new ConfiguredEnum<TEnum> WithDescription(string description)
        {
            base.WithDescription(description);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithConfigIdentifier(string)"/>
        public new ConfiguredEnum<TEnum> WithConfigIdentifier(string identifier)
        {
            base.WithConfigIdentifier(identifier);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModGUID(BaseUnityPlugin)"/>
        public new ConfiguredEnum<TEnum> WithModGUID(BaseUnityPlugin plugin)
        {
            base.WithModGUID(plugin);
            return this;
        }

        ///<inheritdoc cref="ConfiguredVariable.WithModName(BaseUnityPlugin)"/>
        public new ConfiguredEnum<TEnum> WithModName(BaseUnityPlugin plugin)
        {
            base.WithModName(plugin);
            return this;
        }

        /// <inheritdoc cref="ConfiguredVariable{T}.WithConfigChange(ConfiguredVariable{T}.OnConfigChangedDelegate)"/>
        public new ConfiguredEnum<TEnum> WithConfigChange(OnConfigChangedDelegate del)
        {
            base.WithConfigChange(del);
            return this;
        }

        /// <summary>
        /// Chainable method for setting <see cref="ChoiceConfig"/>
        /// </summary>
        public ConfiguredEnum<TEnum> WithChoiceConfig(ChoiceConfig choiceConfig)
        {
            ChoiceConfig = choiceConfig;
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
                var option = ChoiceConfig == null ? new ChoiceOption(ConfigEntry) : new ChoiceOption(ConfigEntry, ChoiceConfig);
                ModSettingsManager.AddOption(option, guid, name);
            }
        }

        /// <inheritdoc cref="ConfiguredVariable(object)"/>
        public ConfiguredEnum(TEnum defaultVal) : base(defaultVal) { }
    }
}