using BepInEx.Configuration;
using HG.Reflection;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        public string ConfigSectionOverride { get; set; }
        
        /// <summary>
        /// An override for the ConfigEntry's Name, if left null, it'll use a "Nicified" version of the field/property's name
        /// </summary>
        public string ConfigNameOverride { get; set; }

        /// <summary>
        /// The description of the Config
        /// </summary>
        public string ConfigDescOverride { get; set; }

        /// <summary>
        /// Returns the ConfigFileIdentifier specified by this ConfigureField attribute.
        /// </summary>
        public string ConfigFileIdentifier => _configFileIdentifier;
        private string _configFileIdentifier;

        /// <summary>
        /// Returns true if the attached MemberInfo is a Field
        /// </summary>
        public bool AttachedMemberIsField => target is FieldInfo;
        
        /// <summary>
        /// Returns the field that's attached to this ConfigureField
        /// </summary>
        public FieldInfo AttachedField => (FieldInfo)target;

        /// <summary>
        /// Returns the property that's attached to this ConfigureField
        /// </summary>
        public PropertyInfo AttachedProperty => (PropertyInfo)target;

        /// <summary>
        /// Returns the member that's attached to this ConfigureField
        /// </summary>
        public MemberInfo AttachedMemberInfo => (MemberInfo)target;

        /// <summary>
        /// The ConfigEntry thats tied to this ConfigureField attribute.
        /// <br>See <see cref="GetConfigEntry{T}"/></br>
        /// </summary>
        public ConfigEntryBase ConfigEntryBase { get; private set; }

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
            catch(Exception e)
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
            ConfigEntryBase = configFile.Bind<T>(GetSection(), GetName(), value, GetDescription());
            var entry = GetConfigEntry<T>();
            entry.SettingChanged += SettingChanged;
            SetValue(ConfigEntryBase.BoxedValue);
            OnConfigured(configFile, value);
        }

        private void SettingChanged(object sender, EventArgs e)
        {
            SetValue(ConfigEntryBase.BoxedValue);
        }

        private void SetValue(object boxedValue)
        {
            if(AttachedMemberIsField)
            {
                AttachedField.SetValue(null, boxedValue);
            }
            else
            {
                var method = AttachedProperty.GetSetMethod();
                method?.Invoke(null, new object[] {boxedValue});
            }
        }

        private string GetSection()
        {
            Type type = AttachedMemberInfo.DeclaringType;
            if (!string.IsNullOrEmpty(ConfigSectionOverride))
            {
                return ConfigSectionOverride;
            }
            return MSUtil.NicifyString(type.Name);
        }

        private string GetName()
        {
            if (!string.IsNullOrEmpty(ConfigNameOverride))
            {
                return ConfigNameOverride;
            }
            return MSUtil.NicifyString(AttachedMemberInfo.Name);
        }

        private string GetDescription()
        {
            if (!string.IsNullOrEmpty(ConfigDescOverride))
            {
                return ConfigDescOverride;
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
        public string ModGUID { get; internal set; }
        /// <summary>
        /// The GUID of the mod that owns this ConfigurableField, Set automatically by the <see cref="ConfigSystem"/>.
        /// </summary>
        public string ModName { get; internal set; }

        protected override void OnConfigured(ConfigFile configFile, object value)
        {
            bool separateEntry = ConfigSystem.ShouldCreateSeparateRiskOfOptionsEntry(configFile);
            string fileName = Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
            var guid = separateEntry ? ModGUID + "." + fileName : ModGUID;
            var name = separateEntry ? ModGUID + "." + fileName : ModName;

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
                    ModSettingsManager.AddOption(new ChoiceOption(ConfigEntryBase), guid, name);
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
