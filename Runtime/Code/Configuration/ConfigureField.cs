using BepInEx.Configuration;
using HG.Reflection;
using RiskOfOptions;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MSU.Config
{
    /// <summary>
    /// The ConfigureFieldAttribute can be used to make a field or property configurable using BepInEx's ConfigSystem.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConfigureFieldAttribute : SearchableAttribute
    {
        /// <summary>
        /// An override for the ConfigEntry's Section, if left null, it'll use a "Nicified" version of the Declaring Type's name
        /// </summary>
        public string configSectionOverride { get; set; }

        /// <summary>
        /// An override for the ConfigEntry's Name, if left null, it'll use a "Nicified" version of the field/property's name
        /// </summary>
        public string configNameOverride { get; set; }

        /// <summary>
        /// The description of the Config
        /// </summary>
        public string configDescOverride { get; set; }

        /// <summary>
        /// Returns the ConfigFileIdentifier specified by this ConfigureField attribute.
        /// </summary>
        public string configFileIdentifier => _configFileIdentifier;
        private string _configFileIdentifier;

        /// <summary>
        /// Returns true if the attached MemberInfo is a Field
        /// </summary>
        public bool attachedMemberIsField => target is FieldInfo;

        /// <summary>
        /// Returns the field that's attached to this ConfigureField
        /// </summary>
        public FieldInfo attachedField => (FieldInfo)target;

        /// <summary>
        /// Returns the property that's attached to this ConfigureField
        /// </summary>
        public PropertyInfo attachedProperty => (PropertyInfo)target;

        /// <summary>
        /// Returns the member that's attached to this ConfigureField
        /// </summary>
        public MemberInfo attachedMemberInfo => (MemberInfo)target;

        /// <summary>
        /// The ConfigEntry thats tied to this ConfigureField attribute.
        /// <br>See <see cref="GetConfigEntry{T}"/></br>
        /// </summary>
        public ConfigEntryBase configEntryBase { get; private set; }

        /// <summary>
        /// Returns <see cref="configEntryBase"/> as a generic ConfigEntry using casting.
        /// </summary>
        /// <typeparam name="T">The type to use for the generic during casting</typeparam>
        /// <returns>The casted ConfigEntry</returns>
        public ConfigEntry<T> GetConfigEntry<T>()
        {
            try
            {
                return configEntryBase == null ? null : (ConfigEntry<T>)configEntryBase;
            }
            catch (Exception e)
            {
                MSULog.Error(e);
                return null;
            }
        }

        /// <summary>
        /// This method gets called when the ConfigureField configures a specific field.
        /// </summary>
        /// <param name="configFile">The ConfigFile that the ConfigEntry got bound to</param>
        /// <param name="value">The value for the field</param>
        protected virtual void OnConfigured(ConfigFile configFile, object value) { }

        internal void ConfigureField<T>(ConfigFile configFile, T value)
        {
            configEntryBase = configFile.Bind<T>(GetSection(), GetName(), value, GetDescription());
            var entry = GetConfigEntry<T>();
            entry.SettingChanged += SettingChanged;
            SetValue(configEntryBase.BoxedValue);
            OnConfigured(configFile, value);
        }

        private void SettingChanged(object sender, EventArgs e)
        {
            SetValue(configEntryBase.BoxedValue);
        }

        private void SetValue(object boxedValue)
        {
            if (attachedMemberIsField)
            {
                attachedField.SetValue(null, boxedValue);
            }
            else
            {
                var method = attachedProperty.GetSetMethod();
                method?.Invoke(null, new object[] { boxedValue });
            }
        }

        private string GetSection()
        {
            Type type = attachedMemberInfo.DeclaringType;
            if (!string.IsNullOrEmpty(configSectionOverride))
            {
                return configSectionOverride;
            }
            return MSUtil.NicifyString(type.Name);
        }

        private string GetName()
        {
            if (!string.IsNullOrEmpty(configNameOverride))
            {
                return configNameOverride;
            }
            return MSUtil.NicifyString(attachedMemberInfo.Name);
        }

        private string GetDescription()
        {
            if (!string.IsNullOrEmpty(configDescOverride))
            {
                return configDescOverride;
            }
            return string.Empty;
        }

        /// <summary>
        /// Constructor for a ConfigureField
        /// </summary>
        /// <param name="fileIdentifier">The ConfigureField's ConfigFileIdentifier</param>
        public ConfigureFieldAttribute(string fileIdentifier)
        {
            _configFileIdentifier = fileIdentifier;
        }
    }

    /// <summary>
    /// <inheritdoc cref="ConfigureFieldAttribute"/>
    /// <para>The RiskOfOptionsConfigureFieldAttribute is an extended version of a <see cref="ConfigureFieldAttribute"/>. Unlike the regular ConfigueFieldAttribute, this attribute will also create a RiskOfOption's option for the field so it can be configured ingame using Risk of Options.</para>
    /// <para>Due to the limitations of Attributes, its not possible to specify OptionConfigs for the RiskOfOptions Options, instead you might want to use a <see cref="ConfiguredVariable"/></para>
    /// </summary>
    public class RiskOfOptionsConfigureFieldAttribute : ConfigureFieldAttribute
    {
        /// <summary>
        /// The GUID of the mod that owns this ConfigurableField, Set automatically by the <see cref="ConfigSystem"/>.
        /// </summary>
        public string modGUID { get; internal set; }
        /// <summary>
        /// The GUID of the mod that owns this ConfigurableField, Set automatically by the <see cref="ConfigSystem"/>.
        /// </summary>
        public string modName { get; internal set; }
        /// <inheritdoc/>

        protected override void OnConfigured(ConfigFile configFile, object value)
        {
            bool separateEntry = ConfigSystem.ShouldCreateSeparateRiskOfOptionsEntry(configFile);
            string fileName = Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
            var guid = separateEntry ? modGUID + "." + fileName : modGUID;
            var name = separateEntry ? modGUID + "." + fileName : modName;

            switch (value)
            {
                case Boolean _bool:
                    ModSettingsManager.AddOption(new CheckBoxOption(GetConfigEntry<bool>()), guid, name);
                    break;
                case float _float:
                    ModSettingsManager.AddOption(new SliderOption(GetConfigEntry<float>()), guid, name);
                    break;
                case int _int:
                    ModSettingsManager.AddOption(new IntSliderOption(GetConfigEntry<int>()), guid, name);
                    break;
                case string _string:
                    ModSettingsManager.AddOption(new StringInputFieldOption(GetConfigEntry<string>()), guid, name);
                    break;
                case Color _color:
                    ModSettingsManager.AddOption(new ColorOption(GetConfigEntry<Color>()), guid, name);
                    break;
                case Enum _enum:
                    ModSettingsManager.AddOption(new ChoiceOption(configEntryBase), guid, name);
                    break;
                case KeyboardShortcut _keyboardShortcut:
                    ModSettingsManager.AddOption(new KeyBindOption(GetConfigEntry<KeyboardShortcut>()), guid, name);
                    break;
            }
        }

        /// <summary>
        /// Constructor for a ConfigureField
        /// </summary>
        /// <param name="fileIdentifier">The ConfigureField's ConfigFileIdentifier</param>
        public RiskOfOptionsConfigureFieldAttribute(string fileIdentifier) : base(fileIdentifier)
        {
        }
    }
}
