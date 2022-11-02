using System;
using System.Globalization;
using System.Reflection;

namespace Moonstorm
{
    /// <summary>
    /// A StatType for the Token Modifier, this is used to modify the value for the token.
    /// </summary>
    public enum StatTypes : int
    {
        /// <summary>
        /// No changes are made to the field/property value for the token formatting
        /// </summary>
        Default,
        /// <summary>
        /// The value of the field/property is multiplied by 100
        /// </summary>
        [Obsolete("Use MultiplyByN and set the extraData field to 100")]
        Percentage,
        /// <summary>
        /// The value of the field/property is divided by 2
        /// </summary>
        [Obsolete("Use DivideByN and set the extraData field to 2")]
        DivideBy2,
        /// <summary>
        /// The value of this field/property is divided by N, where N is a float that'll be parsed from <see cref="TokenModifierAttribute.extraData"/>
        /// </summary>
        DivideByN,
        /// <summary>
        /// The value of this field/property is multiplied by N, where N is a float that'll be parsed from <see cref="TokenModifierAttribute.extraData"/>
        /// </summary>
        MultiplyByN,
        /// <summary>
        /// N is added to the value of this field/property, where N is a float that'll be parsed from <see cref="TokenModifierAttribute.extraData"/>
        /// </summary>
        AddN,
        /// <summary>
        /// N is substracted to the value of this field/property, where N is a float that'll be parsed from <see cref="TokenModifierAttribute.extraData"/>
        /// </summary>
        SubtractN,
    }

    /// <summary>
    /// Declares that the value from a field must be used for formatting a language token
    /// <para>You should add your mod to the <see cref="TokenModifierManager"/> with <seealso cref="TokenModifierManager.AddToManager"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class TokenModifierAttribute : Attribute
    {
        /// <summary>
        /// The LanguageToken to be formatted
        /// </summary>
        public string langToken;
        /// <summary>
        /// The type of stat this field has
        /// </summary>
        public StatTypes statType;
        /// <summary>
        /// The index used during formatting process
        /// </summary>
        public int formatIndex;
        /// <summary>
        /// Extra data to be used during formatting, should be used depending on the chosen StatType
        /// <para>Examples of stat types that use this field: <see cref="StatTypes.DivideByN"/>, <see cref="StatTypes.MultiplyByN"/></para>
        /// </summary>
        public string extraData;

        private object valueForFormatting;

        /// <summary>
        /// Constructor for the TokenModifierAttribute
        /// </summary>
        public TokenModifierAttribute(string langToken, StatTypes statType, int formatIndex = 0)
        {
            this.langToken = langToken;
            this.statType = statType;
            this.formatIndex = formatIndex;
        }
        /// <inheritdoc cref="TokenModifierAttribute.TokenModifierAttribute(string, StatTypes, int)"/>
        public TokenModifierAttribute(string langToken, StatTypes statType, int formatIndex = 0, string extraData = "")
        {
            this.langToken = langToken;
            this.statType = statType;
            this.formatIndex = formatIndex;
            this.extraData = extraData;
        }

        internal (object, int) GetFormatting(PropertyInfo propertyInfo)
        {
            if (valueForFormatting != null)
            {
                return (valueForFormatting, formatIndex);
            }

            var getMethod = propertyInfo.GetMethod;
            object value = getMethod.Invoke(null, null);
            if (value != null && IsNumber(value))
            {
                switch (statType)
                {
                    case StatTypes.Default:
                        valueForFormatting = value;
                        return (valueForFormatting, formatIndex);
                    case StatTypes.Percentage:
                        extraData = "100";
                        valueForFormatting = MultiplyByN(CastToFloat(value));
                        return (valueForFormatting, formatIndex);
                    case StatTypes.DivideBy2:
                        extraData = "2";
                        valueForFormatting = DivideByN(CastToFloat(value));
                        return (valueForFormatting, formatIndex);
                    case StatTypes.MultiplyByN:
                        valueForFormatting = MultiplyByN(CastToFloat(value));
                        return (valueForFormatting, formatIndex);
                    case StatTypes.DivideByN:
                        valueForFormatting = DivideByN(CastToFloat(value));
                        return (valueForFormatting, formatIndex);
                    case StatTypes.AddN:
                        valueForFormatting = AddN(CastToFloat(value));
                        return (valueForFormatting, formatIndex);
                    case StatTypes.SubtractN:
                        valueForFormatting = SubtractN(CastToFloat(value));
                        return (valueForFormatting, formatIndex);
                }
            }
            else
            {
                MSULog.Error($"The type {propertyInfo.PropertyType} is not a number, the {nameof(TokenModifierAttribute)} attribute should only be used on fields/properties that are numbers!");
            }
            return (null, 0);
        }

        internal (object, int) GetFormatting(FieldInfo fieldInfo)
        {
            if (valueForFormatting != null)
            {
                return (valueForFormatting, formatIndex);
            }
            else
            {
                object value = fieldInfo.GetValue(null);
                if (value != null && IsNumber(value))
                {
                    switch (statType)
                    {
                        case StatTypes.Default:
                            valueForFormatting = value;
                            return (valueForFormatting, formatIndex);
                        case StatTypes.Percentage:
                            extraData = "100";
                            valueForFormatting = MultiplyByN(CastToFloat(value));
                            return (valueForFormatting, formatIndex);
                        case StatTypes.DivideBy2:
                            extraData = "2";
                            valueForFormatting = DivideByN(CastToFloat(value));
                            return (valueForFormatting, formatIndex);
                        case StatTypes.MultiplyByN:
                            valueForFormatting = MultiplyByN(CastToFloat(value));
                            return (valueForFormatting, formatIndex);
                        case StatTypes.DivideByN:
                            valueForFormatting = DivideByN(CastToFloat(value));
                            return (valueForFormatting, formatIndex);
                        case StatTypes.AddN:
                            valueForFormatting = AddN(CastToFloat(value));
                            return (valueForFormatting, formatIndex);
                        case StatTypes.SubtractN:
                            valueForFormatting = SubtractN(CastToFloat(value));
                            return (valueForFormatting, formatIndex);
                    }
                }
                else
                {
                    MSULog.Error($"The type {fieldInfo.FieldType} is not a number, the {nameof(TokenModifierAttribute)} attribute should only be used on fields that are numbers!");
                }
                return (null, 0);
            }
        }

        private float CastToFloat(object obj)
        {
            float value = Convert.ToSingle(obj, CultureInfo.InvariantCulture);
            return value;
        }

        private object MultiplyByN(float number)
        {
            var coef = float.Parse(extraData, CultureInfo.InvariantCulture);
            float num = number * coef;
            return num;
        }

        private object DivideByN(float number)
        {
            var dividend = float.Parse(extraData, CultureInfo.InvariantCulture);
            float num = number / dividend;
            return num;
        }

        private object AddN(float number)
        {
            var addend = float.Parse(extraData, CultureInfo.InvariantCulture);
            float num = number + addend;
            return num;
        }

        private object SubtractN(float number)
        {
            var substrahend = float.Parse(extraData, CultureInfo.InvariantCulture);
            float num = number - substrahend;
            return num;
        }

        private static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }
    }
}
