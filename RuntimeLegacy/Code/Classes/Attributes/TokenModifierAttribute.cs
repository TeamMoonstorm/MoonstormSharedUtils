using BepInEx.Configuration;
using HG.Reflection;
using Moonstorm.Config;
using System;
using System.Globalization;
using System.Reflection;

namespace Moonstorm
{

    public enum StatTypes : int
    {
        Default,
        DivideByN,
        MultiplyByN,
        AddN,
        SubtractN,
        ModuloN,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class TokenModifierAttribute : SearchableAttribute
    {
        public string langToken;
        public StatTypes statType;
        public int formatIndex;

        public float operationData = float.NaN;

        private object valueForFormatting;

        public TokenModifierAttribute(string langToken, StatTypes statType, int formatIndex = 0)
        {
            this.langToken = langToken;
            this.statType = statType;
            this.formatIndex = formatIndex;
        }
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
