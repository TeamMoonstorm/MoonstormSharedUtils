using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Moonstorm
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigurableFieldAttribute : Attribute
    {
        public string ConfigSection { get; set; }
        public string ConfigName { get; set; }
        public string ConfigDesc { get; set; }
        public string configFileIdentifier;

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