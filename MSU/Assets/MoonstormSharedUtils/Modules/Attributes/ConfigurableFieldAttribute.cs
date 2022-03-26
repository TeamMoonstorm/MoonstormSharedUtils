using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Moonstorm
{
    /// <summary>
    /// The Configurable Field Attribute can be used to make a field Configurable using BepInEx's Config System.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigurableFieldAttribute : Attribute
    {
        /// <summary>
        /// The Section of the Config, if left null, it'll use the declaring type's name.
        /// </summary>
        public string ConfigSection { get; set; }
        
        /// <summary>
        /// The Name of the Config, if left null, it'll use the Field's name.
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// The Description of the Config, if left null, it'll use a generic description.
        /// </summary>
        public string ConfigDesc { get; set; }
        
        /// <summary>
        /// The identifier of a config file to use for binding.
        /// If left null, it'll use the mod's main config file.
        /// </summary>
        public string configFileIdentifier;

        /// <summary>
        /// Creates a new instance of the Configurable Field Attribute.
        /// </summary>
        /// <param name="fileIdentifier">The config identifier</param>
        public ConfigurableFieldAttribute(string fileIdentifier = null)
        {
            configFileIdentifier = fileIdentifier;
        }

        public string GetSection(Type type)
        {
            if (!string.IsNullOrEmpty(ConfigSection))
            {
                return ConfigSection;
            }
            return Nicify(type.Name);
        }

        public string GetName(FieldInfo field)
        {
            if (!string.IsNullOrEmpty(ConfigName))
            {
                return ConfigName;
            }
            return Nicify(field.Name);
        }

        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(ConfigDesc))
            {
                return ConfigDesc;
            }
            return $"Configure this value";
        }

        private string Nicify(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            string text = string.Empty;
            if(char.IsLower(name[0]))
            {
                text = char.ToUpper(text[0]) + text.Substring(1);
            }

            List<char> newText = new List<char>();
            for (int i = 0; i < text.Length; i++)
            {
                if (i == 0)
                {
                    newText.Add(text[i]);
                    continue;
                }
                char character = text[i];
                if (char.IsUpper(character))
                {
                    newText.Add(' ');
                    newText.Add(character);
                    continue;
                }
                newText.Add(character);
            }
            return new string(newText.ToArray());
        }
    }
}