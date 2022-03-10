using System;
using System.Reflection;

namespace Moonstorm
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigurableField : Attribute
    {
        public string ConfigSection { get; set; }
        public string ConfigName { get; set; }
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