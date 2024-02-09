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
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConfigurableFieldAttribute : SearchableAttribute
    {

        public string ConfigSection { get; set; }


        public string ConfigName { get; set; }


        public string ConfigDesc { get; set; }


        public string ConfigFileIdentifier => configFileIdentifier;
        private string configFileIdentifier;


        public FieldInfo Field => (FieldInfo)target;


        public ConfigEntryBase ConfigEntryBase { get; private set; }


        public ConfigurableFieldAttribute(string fileIdentifier = null)
        {
            configFileIdentifier = fileIdentifier;
        }

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

    public class RooConfigurableFieldAttribute : ConfigurableFieldAttribute
    {

        public string OwnerGUID { get; internal set; }


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
                case KeyboardShortcut _keyboardShortcut:
                    ModSettingsManager.AddOption(new KeyBindOption(GetConfigEntry<KeyboardShortcut>()), guid, ownerName);
                    break;
            }
        }

        public RooConfigurableFieldAttribute(string identifier = null) : base(identifier)
        {

        }
    }
}