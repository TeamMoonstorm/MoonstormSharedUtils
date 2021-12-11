using System;
using System.Globalization;
using System.Reflection;

namespace Moonstorm
{
    public enum StatTypes : int
    {
        Default,
        Percentage,
        DivideBy2,
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TokenModifier : Attribute
    {
        public string langToken;
        public StatTypes statType;
        public int formatIndex;

        private object valueForFormatting;
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
