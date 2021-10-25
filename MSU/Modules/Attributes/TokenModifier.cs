using System;
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
                if (fieldValue != null && fieldValue.IsNumber())
                {
                    switch(statType)
                    {
                        case StatTypes.Default:
                            valueForFormatting = fieldValue;
                            return (valueForFormatting, formatIndex);
                        case StatTypes.Percentage:
                            valueForFormatting = ToPercent(fieldValue);
                            return (valueForFormatting, formatIndex);
                        case StatTypes.DivideBy2:
                            valueForFormatting = DivideBy2(fieldValue);
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

        private object ToPercent(dynamic obj)
        {
            return obj * 100;
        }

        private object DivideBy2(dynamic obj)
        {
            return obj / 2;
        }
    }
}
