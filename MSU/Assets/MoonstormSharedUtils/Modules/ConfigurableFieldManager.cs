using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// Class for managing ConfigurableField attributes
    /// </summary>
    public static class ConfigurableFieldManager
    {
        private static bool initialized = false;

        private static List<(List<Type>, ConfigFile)> typesToConfigure = new List<(List<Type>, ConfigFile)>();

        private static List<(List<FieldInfo>, ConfigFile)> fieldsToConfigure = new List<(List<FieldInfo>, ConfigFile)>();

        [SystemInitializer()]
        private static void Init()
        {
            initialized = true;

            MSULog.Info($"Initializing ConfigurableFieldManager");
            RoR2Application.onLoad += ConfigureFields;
        }

        /// <summary>
        /// Adds a mod to the ConfigurableField manager
        /// <para>Will automatically look for Types that have fields with ConfigurableField attribute and add them for configuration</para>
        /// </summary>
        /// <param name="configFile">Your Mod's ConfigFile</param>
        public static void AddMod(ConfigFile configFile)
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            if(initialized)
            {
                MSULog.Warning($"Cannot add {assembly.GetName().Name} to the List as the configurable field manager has already been initialized.");
                return;
            }

            MSULog.Info($"Adding mod {assembly.GetName().Name} to the configurable field manager");

            List<FieldInfo> fields = new List<FieldInfo>();
            foreach(Type type in assembly.GetTypes().Where(type => type.GetCustomAttribute<DisabledContent>() == null))
            {
                try
                {
                    var validFields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                                          .Where(field => field.GetCustomAttribute<ConfigurableField>() != null)
                                          .ToList();

                    if(validFields.Count > 0)
                    {
                        fields.AddRange(validFields);
                    }
                }
                catch(Exception e) { MSULog.Error(e); }
            }

            if(fields.Count == 0)
            {
                MSULog.Warning($"Found no types with fields that have the {nameof(ConfigurableField)} attribute within {assembly.GetName().Name}");
                return;
            }

            MSULog.Debug($"Found a total of {fields.Count} fields that have the {nameof(ConfigurableField)} attribute");
            var tuple = (fields, configFile);
            fieldsToConfigure.Add(tuple);
        }

        private static void ConfigureFields()
        {
            List<FieldInfo> count = new List<FieldInfo>();
            fieldsToConfigure.ForEach(field => count.AddRange(field.Item1));
            MSULog.Info($"Configuring a total of {count.Count} Fields.");

            foreach(var (fields, config) in fieldsToConfigure)
            {
                try
                {
                    foreach(FieldInfo field in fields)
                    {
                        try
                        {
                            ConfigureField(field, config);
                        }
                        catch(Exception e) { MSULog.Error(e); }
                    }
                }
                catch(Exception e) { MSULog.Error(e); }
            }
        }

        private static void ConfigureField(FieldInfo field, ConfigFile config)
        {
            MSULog.Debug($"Configuring {field.Name}");

            var attribute = field.GetCustomAttribute<ConfigurableField>(true);

            switch(field.GetValue(null))
            {
                case String _text: Bind<String>(field, config, _text, attribute); break;
                case Boolean _bool: Bind<Boolean>(field, config, _bool, attribute); break;
                case Byte _byte: Bind<Byte>(field, config, _byte, attribute); break;
                case SByte _sbyte: Bind<SByte>(field, config, _sbyte, attribute); break;
                case Int16 _int16: Bind<Int16>(field, config, _int16, attribute); break;
                case UInt16 _uint16: Bind<UInt16>(field, config, _uint16, attribute); break;
                case Int32 _int32: Bind<Int32>(field, config, _int32, attribute); break;
                case UInt32 _uint32: Bind<UInt32>(field, config, _uint32, attribute); break;
                case Int64 _int64: Bind<Int64>(field, config, _int64, attribute); break;
                case UInt64 _uint64: Bind<UInt64>(field, config, _uint64, attribute); break;
                case Single _single: Bind<Single>(field, config, _single, attribute); break;
                case Double _double: Bind<Double>(field, config, _double, attribute); break;
                case Decimal _decimal: Bind<Decimal>(field, config, _decimal, attribute); break;
                case Enum _enum: Bind<Enum>(field, config, _enum, attribute); break;
                case Color _color: Bind<Color>(field, config, _color, attribute); break;
                case Vector2 _vector2: Bind<Vector2>(field, config, _vector2, attribute); break;
                case Vector3 _vector3: Bind<Vector3>(field, config, _vector3, attribute); break;
                case Vector4 _vector4: Bind<Vector4>(field, config, _vector4, attribute); break;
                case Quaternion _quaternion: Bind<Quaternion>(field, config, _quaternion, attribute); break;
            }
        }

        private static void Bind<T>(FieldInfo field, ConfigFile config, T value, ConfigurableField attribute)
        {
            Type t = field.DeclaringType;
            field.SetValue(t, config.Bind<T>(attribute.GetSection(t), attribute.GetName(field), value, attribute.GetDescription()).Value);
        }
    }
}
