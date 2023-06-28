using BepInEx.Configuration;
using HG.Reflection;
using Moonstorm.Config;
using RiskOfOptions;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// The Configurable Field Attribute can be used to make a field Configurable using BepInEx's Config System.
    /// <para>You should add your mod to the <see cref="ConfigSystem"/> with <seealso cref="ConfigSystem.AddMod(BepInEx.BaseUnityPlugin)"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConfigurableFieldAttribute : SearchableAttribute
    {
        /// <summary>
        /// The Section of the Config, if left null, it'll use a "Nicified" version of the declaring type's name.
        /// </summary>
        public string ConfigSection { get; set; }

        /// <summary>
        /// The Name of the Config, if left null, it'll use a "Nicified" version of the Field's name.
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// The Description of the Config, if left null, it'll use a generic description.
        /// </summary>
        public string ConfigDesc { get; set; }

        /// <summary>
        /// During the <see cref="ConfigSystem"/> initialization, the ConfigSystem will try to bind the config to the config file with this identifier.
        /// </summary>
        public string ConfigFileIdentifier => configFileIdentifier;
        private string configFileIdentifier;

        /// <summary>
        /// The FieldInfo that's attached to this Attribute.
        /// </summary>
        public FieldInfo Field => (FieldInfo)target;

        /// <summary>
        /// The ConfigEntry that's tied to this ConfigurableField. See <see cref="GetConfigEntry{T}"/> as well.
        /// </summary>
        public ConfigEntryBase ConfigEntryBase { get; private set; }

        /// <summary>
        /// Creates a new instance of the Configurable Field Attribute.
        /// </summary>
        /// <param name="fileIdentifier">The config identifier, the config file that has this identifier will be the file used for the binding process</param>
        public ConfigurableFieldAttribute(string fileIdentifier = null)
        {
            configFileIdentifier = fileIdentifier;
        }

        /// <summary>
        /// Returns <see cref="ConfigEntryBase"/> as a generic ConfigEntry using casting.
        /// </summary>
        /// <typeparam name="T">The type to use for the generic during casting</typeparam>
        /// <returns>The casted ConfigEntry</returns>
        public ConfigEntry<T> GetConfigEntry<T>()
        {
            try
            {
                return ConfigEntryBase == null ? null : (ConfigEntry<T>)ConfigEntryBase;
            }
            catch (Exception ex)
            {
                MSULog.Error(ex);
                return null;
            }
        }

        internal void ConfigureField<T>(ConfigFile configFile, T value)
        {
            ConfigEntryBase = configFile.Bind<T>(GetSection(), GetName(), value, GetDescription());
            var entry = GetConfigEntry<T>();
            entry.SettingChanged += SettingChanged;
            SetValue(ConfigEntryBase.BoxedValue);
            OnConfigured(configFile, value);
        }

        /// <summary>
        /// When the ConfigSystem configures this ConfigurableFieldAttribute, OnConfigured gets called
        /// </summary>
        /// <param name="configFile">The ConfigFile that this ConfigurableFieldAttribute was bound to</param>
        /// <param name="value">The value for this ConfigurableField's field</param>
        protected virtual void OnConfigured(ConfigFile configFile, object value) { }
        private void SettingChanged(object sender, EventArgs e)
        {
            SetValue(ConfigEntryBase.BoxedValue);
        }

        private void SetValue(object boxedValue)
        {
            Field.SetValue(Field.DeclaringType, boxedValue);
        }

        private string GetSection()
        {
            Type type = Field.DeclaringType;
            if (!string.IsNullOrEmpty(ConfigSection))
            {
                return ConfigSection;
            }
            return MSUtil.NicifyString(type.Name);
        }

        private string GetName()
        {
            if (!string.IsNullOrEmpty(ConfigName))
            {
                return ConfigName;
            }
            return MSUtil.NicifyString(Field.Name);
        }

        private string GetDescription()
        {
            if (!string.IsNullOrEmpty(ConfigDesc))
            {
                return ConfigDesc;
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// <inheritdoc cref="ConfigurableFieldAttribute"/>
    /// <para>The <see cref="RooConfigurableFieldAttribute"/>, otherwise known as the <see cref="RiskOfOptions"/>ConfigurableFieldAttribute, is an extended version of the <see cref="ConfigurableFieldAttribute"/> that allows the created ConfigEntry to be shown on the RiskOfOptions' Options menu.</para>
    /// <para>It does this by looking the Type of the field and adding generic options to the <see cref="ModSettingsManager"/></para>
    /// <para>If you need more control over the Options on your Options menu, consider using <see cref="ConfigurableVariable{T}"/> and it's derivatives</para>
    /// </summary>
    public class RooConfigurableFieldAttribute : ConfigurableFieldAttribute
    {
        /// <summary>
        /// The GUID of the mod that owns this ConfigurableField, Set automatically by the <see cref="ConfigSystem"/>.
        /// </summary>
        public string OwnerGUID { get; internal set; }

        /// <summary>
        /// The Name of the mod that owns this ConfigurableField, Set automatically by the <see cref="ConfigSystem"/>.
        /// </summary>
        public string OwnerName { get; internal set; }

        protected override void OnConfigured(ConfigFile file, object value)
        {
            bool separateEntry = ConfigSystem.configFilesWithSeparateRooEntries.Contains(file);
            string fileName = Path.GetFileNameWithoutExtension(file.ConfigFilePath);
            var guid = separateEntry ? OwnerGUID + "." + fileName : OwnerGUID;
            var ownerName = separateEntry ? OwnerName + "." + fileName : OwnerName;
            switch (value)
            {
                case Boolean _bool:
                    ModSettingsManager.AddOption(new CheckBoxOption(GetConfigEntry<bool>()), guid, ownerName);
                    break;
                case float _float:
                    ModSettingsManager.AddOption(new SliderOption(GetConfigEntry<float>()), guid, ownerName);
                    break;
                case int _int:
                    ModSettingsManager.AddOption(new IntSliderOption(GetConfigEntry<int>()), guid, ownerName);
                    break;
                case string _string:
                    ModSettingsManager.AddOption(new StringInputFieldOption(GetConfigEntry<string>()), guid, ownerName);
                    break;
                case Color _color:
                    ModSettingsManager.AddOption(new ColorOption(GetConfigEntry<Color>()), guid, ownerName);
                    break;
                case Enum _enum:
                    ModSettingsManager.AddOption(new ChoiceOption(ConfigEntryBase), guid, ownerName);
                    break;
            }
        }

        /// <summary>
        /// Creates a new instance of the Risk of Options Configurable Field Attribute.
        /// </summary>
        /// <param name="identifier"><inheritdoc cref="ConfigurableFieldAttribute.ConfigurableFieldAttribute(string)"/></param>
        public RooConfigurableFieldAttribute(string identifier = null) : base(identifier)
        {

        }
    }
}