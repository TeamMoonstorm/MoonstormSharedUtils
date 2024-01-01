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
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConfigureFieldAttribute : SearchableAttribute
    {
        public string ConfigSectionOverride { get; set; }
        
        public string ConfigNameOverride { get; set; }

        public string ConfigDescOverride { get; set; }

        public string ConfigFileIdentifier => _configFileIdentifier;
        private string _configFileIdentifier;

        public bool AttachedMemberIsField => target is FieldInfo;
        
        public FieldInfo AttachedField => (FieldInfo)target;

        public PropertyInfo AttachedProperty => (PropertyInfo)target;

        public MemberInfo AttachedMemberInfo => (MemberInfo)target;

        public ConfigEntryBase ConfigEntryBase { get; private set; }

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

        public ConfigureFieldAttribute(string fileIdentifier)
        {
            _configFileIdentifier = fileIdentifier;
        }
    }

    public class RiskOfOptionsConfigureFieldAttribute : ConfigureFieldAttribute
    {
        public string ModGUID { get; internal set; }
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
        public RiskOfOptionsConfigureFieldAttribute(string fileIdentifier) : base(fileIdentifier)
        {
        }
    }
}
