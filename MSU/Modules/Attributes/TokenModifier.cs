using System;
using System.Globalization;
using System.Reflection;

namespace Moonstorm
{
    /// <summary>
    /// Enum for modifying the value of a field for use in tokens
    /// </summary>
    public enum StatTypes : int
    {
        /// <summary>
        /// No changes are made to the value in the field
        /// </summary>
        Default,
        /// <summary>
        /// The value of the field gets multiplied by 100
        /// </summary>
        Percentage,
        /// <summary>
        /// The value of the field gets divided by 2
        /// </summary>
        DivideBy2,
    }

    /// <summary>
    /// Declares that the value from a field must be used for formatting a language token
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TokenModifier : Attribute
    {
        public string langToken;
        public StatTypes statType;
        public int formatIndex;

        private object valueForFormatting;

        /// <summary>
        /// Initialize a TokenModifier
        /// </summary>
        /// <param name="langToken">The key for the language token to modify. ej: ITEM_SYRINGE_NAME</param>
        /// <param name="statType">A special operation to do on the field's value.</param>
        /// <param name="formatIndex">The formatting index that corresponds to this token modifier</param>
        public TokenModifier(string langToken, StatTypes statType, int formatIndex = 0)
        {
            this.langToken = langToken;
            this.statType = statType;
            this.formatIndex = formatIndex;
        }

        public (object, int) GetFormatting(FieldInfo fieldInfo)
        {
            if (valueForFormatting != null)
            {
                return (valueForFormatting, formatIndex);
            }
            else
            {
                object fieldValue = fieldInfo.GetValue(null);
                if (fieldValue != null && IsNumber(fieldValue))
                {
                    switch (statType)
                    {
                        case StatTypes.Default:
                            valueForFormatting = fieldValue;
                            return (valueForFormatting, formatIndex);
                        case StatTypes.Percentage:
                            valueForFormatting = ToPercent(CastToFloat(fieldValue));
                            return (valueForFormatting, formatIndex);
                        case StatTypes.DivideBy2:
                            valueForFormatting = DivideBy2(CastToFloat(fieldValue));
                            return (valueForFormatting, formatIndex);
                    }
                }
                else
                {
                    MSULog.LogE($"The type {fieldInfo.FieldType} is not a number, the {nameof(TokenModifier)} attribute should only be used on fields that are numbers!");
                }
                return (null, 0);
            }
        }

        private float CastToFloat(object obj)
        {
            float value = Convert.ToSingle(obj, CultureInfo.InvariantCulture);
            return value;
        }

        private object ToPercent(float number)
        {
            float num = number * 100;
            return num;
        }

        private object DivideBy2(float number)
        {
            float num = number / 2;
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
