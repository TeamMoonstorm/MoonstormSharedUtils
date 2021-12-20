using System;
using System.Reflection;

namespace Moonstorm
{
    /// <summary>
    /// Declares that a field can be configured using the mod's Config File
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigurableField : Attribute
    {
        /// <summary>
        /// Write your custom section here, leaving this null will use the declaring type's name
        /// </summary>
        public string ConfigSection { get; set; }
        /// <summary>
        /// Write a custom config name, leaving this null will use the field's name
        /// </summary>
        public string ConfigName { get; set; }
        /// <summary>
        /// Write a custom config description, leaving this null will use a generic description
        /// </summary>
        public string ConfigDesc { get; set; }

        public string GetSection(Type type)
        {
            if (!string.IsNullOrEmpty(ConfigSection))
            {
                return ConfigSection;
            }
            return type.Name;
        }

        public string GetName(FieldInfo field)
        {
            if (!string.IsNullOrEmpty(ConfigName))
            {
                return ConfigName;
            }
            return field.Name;
        }

        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(ConfigDesc))
            {
                return ConfigDesc;
            }
            return $"Configure this value";
        }
    }
}