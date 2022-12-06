using BepInEx.Configuration;
using HG.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RiskOfOptions;

namespace Moonstorm
{
    /// <summary>
    /// The Configurable Field Attribute can be used to make a field Configurable using BepInEx's Config System.
    /// <para>You should add your mod to the <see cref="ConfigurableFieldManager"/> with <seealso cref="ConfigurableFieldManager.AddMod(BepInEx.BaseUnityPlugin)"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
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
        /// During the <see cref="ConfigurableFieldManager"/> initialization, the ConfigurableFieldManager will try to bind the config to the config file with this identifier.
        /// </summary>
        public string ConfigFileIdentifier => configFileIdentifier;
        private string configFileIdentifier;

        /// <summary>
        /// The FieldInfo that's attached to this Attribute.
        /// </summary>
        public FieldInfo Field => (FieldInfo)target;

        /// <summary>
        /// The ConfigEntry that's tied to this ConfigurableField.
        /// <see cref="GetConfigEntry{T}"/>
        /// </summary>
        public ConfigEntryBase ConfigEntryBase { get; private set; }

        /// <summary>
        /// Creates a new instance of the Configurable Field Attribute.
        /// </summary>
        /// <param name="fileIdentifier">The config identifier</param>
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
            return ConfigEntryBase == null ? null : (ConfigEntry<T>)ConfigEntryBase;
        }

        internal void ConfigureField<T>(ConfigFile configFile, T value)
        {
            ConfigEntryBase = configFile.Bind<T>(GetSection(), GetName(), value, GetDescription());

            var entry = GetConfigEntry<T>();
            entry.SettingChanged += SetValue;
        }

        private void SetValue(object sender, EventArgs e)
        {
            var newVal = ConfigEntryBase.BoxedValue;
            Field.SetValue(Field.DeclaringType, newVal);
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
            return $"Configure this value";
        }
    }
}