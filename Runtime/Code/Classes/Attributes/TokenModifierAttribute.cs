using BepInEx;
using BepInEx.Configuration;
using HG.Reflection;
using Moonstorm.Config;
using R2API.Utils;
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
        /// <summary>
        /// The value of this field/property is divided by N, and the remainder of the operation will be the token's value.
        /// </summary>
        ModuloN,
    }

    /// <summary>
    /// Declares that the value from a field must be used for formatting a language token
    /// <para>You should add your mod to the <see cref="TokenModifierManager"/> with <seealso cref="TokenModifierManager.AddToManager"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class TokenModifierAttribute : SearchableAttribute
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

        [Obsolete("Creating floats from strings is error prone, use \"operationData\" instead.")]
        public string extraData;

        /// <summary>
        /// Operation data to be used during formatting, should be used depending on the chosen StatType
        /// <para>Examples of StatTypes that use this field: <see cref="StatTypes.DivideByN"/>, <see cref="StatTypes.MultiplyByN"/></para>
        /// </summary>
        public float operationData = float.NaN;

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
            this.operationData = float.Parse(extraData, CultureInfo.InvariantCulture);
        }
        public TokenModifierAttribute(string langToken, StatTypes statType, int formatIndex = 0, float operationData = 1f)
        {
            this.langToken = langToken;
            this.statType = statType;
            this.formatIndex = formatIndex;
            this.operationData = operationData;
        }

        internal object GetFormattingValue()
        {
            object value = null;
            if (target is FieldInfo fi)
            {
                value = fi.GetValue(null);
            }
            else if (target is PropertyInfo pi)
            {
                value = pi.GetMethod?.Invoke(null, null);
            }

            if (!extraData.IsNullOrWhiteSpace() && float.IsNaN(operationData))
            {
                operationData = float.Parse(extraData, CultureInfo.InvariantCulture);
                extraData = string.Empty;
            }

            Type valueType = value.GetType();
            if (valueType.IsSubclassOf(typeof(ConfigurableVariable)))
            {
                PropertyInfo ConfigEntryBase = valueType.GetProperty(nameof(ConfigurableVariable.ConfigEntryBase), BindingFlags.Public | BindingFlags.Instance);
                var cfg = (ConfigEntryBase)ConfigEntryBase.GetGetMethod().Invoke(value, null);
                value = cfg.BoxedValue;
            }

            if (value != null && IsNumber(value))
            {
                switch (statType)
                {
                    case StatTypes.Default:
                        valueForFormatting = value;
                        return valueForFormatting;
                    case StatTypes.Percentage:
                        operationData = 100f;
                        valueForFormatting = MultiplyByN(CastToFloat(value));
                        return valueForFormatting;
                    case StatTypes.DivideBy2:
                        operationData = 2f;
                        valueForFormatting = DivideByN(CastToFloat(value));
                        return valueForFormatting;
                    case StatTypes.MultiplyByN:
                        valueForFormatting = float.IsNaN(operationData) ? value : MultiplyByN(CastToFloat(value));
                        return valueForFormatting;
                    case StatTypes.DivideByN:
                        valueForFormatting = float.IsNaN(operationData) ? value : DivideByN(CastToFloat(value));
                        return valueForFormatting;
                    case StatTypes.AddN:
                        valueForFormatting = float.IsNaN(operationData) ? value : AddN(CastToFloat(value));
                        return valueForFormatting;
                    case StatTypes.SubtractN:
                        valueForFormatting = float.IsNaN(operationData) ? value : SubtractN(CastToFloat(value));
                        return valueForFormatting;
                    case StatTypes.ModuloN:
                        valueForFormatting = float.IsNaN(operationData) ? value : ModuloN(CastToFloat(value));
                        return valueForFormatting;
                }
            }
            else
            {
                MSULog.Error($"The Field/Property {target}'s Type is not a number, the {nameof(TokenModifierAttribute)} attribute should only be used on fields/properties that are numbers!");
            }
            return null;
        }

        private float CastToFloat(object obj)
        {
            float value = Convert.ToSingle(obj, CultureInfo.InvariantCulture);
            return value;
        }

        private object MultiplyByN(float number)
        {
            var coef = operationData;
            float num = number * coef;
            return num;
        }

        private object DivideByN(float number)
        {
            var dividend = operationData;
            float num = number / dividend;
            return num;
        }

        private object AddN(float number)
        {
            var addend = operationData;
            float num = number + addend;
            return num;
        }

        private object SubtractN(float number)
        {
            var substrahend = operationData;
            float num = number - substrahend;
            return num;
        }

        private object ModuloN(float number)
        {
            var modulo = operationData;
            var num = number % modulo;
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
