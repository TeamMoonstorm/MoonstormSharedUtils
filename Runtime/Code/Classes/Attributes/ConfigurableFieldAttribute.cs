using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string origName = new string(name.ToCharArray());
            try
            {
                if (string.IsNullOrEmpty(name))
                    return name;

                List<char> nameAsChar = null;
                if (name.StartsWith("m_", System.StringComparison.OrdinalIgnoreCase))
                {
                    nameAsChar = name.Substring("m_".Length).ToList();
                }
                else
                {
                    nameAsChar = name.ToList();
                }

                while (nameAsChar.First() == '_')
                {
                    nameAsChar.RemoveAt(0);
                }
                List<char> newText = new List<char>();
                for (int i = 0; i < nameAsChar.Count; i++)
                {
                    char character = nameAsChar[i];
                    if (i == 0)
                    {
                        if (char.IsLower(character))
                        {
                            newText.Add(char.ToUpper(character));
                        }
                        else
                        {
                            newText.Add(character);
                        }
                        continue;
                    }

                    if (char.IsUpper(character))
                    {
                        newText.Add(' ');
                        newText.Add(character);
                        continue;
                    }
                    newText.Add(character);
                }
                return new String(newText.ToArray());
            }
            catch (Exception e)
            {
                MSULog.Error($"Failed to nicify {origName}: {e}");
                return origName;
            }
        }
    }
}