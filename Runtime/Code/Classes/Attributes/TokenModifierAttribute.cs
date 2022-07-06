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
        /// No changes are made to the field value for the token formatting
        /// </summary>
        Default,
        /// <summary>
        /// The value of the field is multiplied by 100
        /// </summary>
        Percentage,
        /// <summary>
        /// The value of the field is divided by 2
        /// </summary>
        DivideBy2,
    }

    /// <summary>
    /// Declares that the value from a field must be used for formatting a language token
    /// <para>You should add your mod to the <see cref="TokenModifierManager"/> with <seealso cref="TokenModifierManager.AddToManager"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
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

        internal (object, int) GetFormatting(FieldInfo fieldInfo)
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
