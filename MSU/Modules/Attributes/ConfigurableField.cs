using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigurableField : Attribute
{
    public string ConfigSection { get; set; }
    public string ConfigName { get; set; }
    public string ConfigDesc { get; set; }

    public string GetSection(Type type)
    {
        if(!string.IsNullOrEmpty(ConfigSection))
        {
            return ConfigSection;
        }
        return type.Name;
    }

    public string GetName(FieldInfo field)
    {
        if(!string.IsNullOrEmpty(ConfigName))
        {
            return ConfigName;
        }
        return field.Name;
    }

    public string GetDescription()
    {
        if(!string.IsNullOrEmpty(ConfigDesc))
        {
            return ConfigDesc;
        }
        return $"Configure this value";
    }
}
