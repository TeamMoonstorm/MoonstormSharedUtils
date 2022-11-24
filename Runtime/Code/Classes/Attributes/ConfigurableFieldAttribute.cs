using BepInEx.Configuration;
using HG.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        /// Creates a new instance of the Configurable Field Attribute.
        /// </summary>
        /// <param name="fileIdentifier">The config identifier</param>
        public ConfigurableFieldAttribute(string fileIdentifier = null)
        {
            configFileIdentifier = fileIdentifier;
        }

        internal string GetSection()
        {
            FieldInfo field = (FieldInfo)target;
            Type type = field.DeclaringType;
            if (!string.IsNullOrEmpty(ConfigSection))
            {
                return ConfigSection;
            }
            return MSUtil.NicifyString(type.Name);
        }

        internal string GetName()
        {
            FieldInfo field = (FieldInfo)target;
            if (!string.IsNullOrEmpty(ConfigName))
            {
                return ConfigName;
            }
            return MSUtil.NicifyString(field.Name);
        }

        internal string GetDescription()
        {
            if (!string.IsNullOrEmpty(ConfigDesc))
            {
                return ConfigDesc;
            }
            return $"Configure this value";
        }
    }
}