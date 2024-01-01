using BepInEx.Configuration;
using HG.Reflection;
using MSU.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class FormatTokenAttribute : SearchableAttribute
    {
        public enum OperationTypeEnum : int
        {
            DivideByN,
            MultiplyByN,
            AddN,
            SubtractN,
            ModuloN,
        }
        public string LanguageToken { get; private set; }
        public OperationTypeEnum? OperationType { get; private set; }
        public float? OperationData { get; private set; }
        public int FormattingIndex { get; private set; }

        private object _cachedFormattingValue;

        internal object GetFormattingValue()
        {
            object value = null;
            if(target is FieldInfo fi)
            {
                value = fi.GetValue(null);
            }
            else if(target is PropertyInfo pi)
            {
                value = pi.GetMethod?.Invoke(null, null);
            }
            else
            {
                throw new InvalidOperationException("FormatTokenAttribute is only valid in Fields and Properties");
            }

            Type valueType = value.GetType();

            if(valueType.IsSubclassOf(typeof(ConfiguredVariable)))
            {
                PropertyInfo configEntryBase = valueType.GetProperty(nameof(ConfiguredVariable.ConfigEntryBase), BindingFlags.Public | BindingFlags.Instance);
                var cfg = (ConfigEntryBase)configEntryBase.GetMethod?.Invoke(value, null);
                value = cfg.BoxedValue;
            }

            if (value == null)
                return string.Empty;

            if(IsNumber(value))
            {
                if(OperationType.HasValue && OperationData.HasValue)
                {
                    switch(OperationType.Value)
                    {
                        case OperationTypeEnum.MultiplyByN:
                            _cachedFormattingValue = MultiplyByN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.DivideByN:
                            _cachedFormattingValue = DivideByN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.AddN:
                            _cachedFormattingValue = AddN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.ModuloN:
                            _cachedFormattingValue = ModuloN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.SubtractN:
                            _cachedFormattingValue = SubtractN(CastToFloat(value));
                            break;
                    }
                }
                _cachedFormattingValue = value;
            }
            else
            {
                _cachedFormattingValue = value;
            }
            return _cachedFormattingValue;
        }

        private float CastToFloat(object obj)
        {
            return Convert.ToSingle(obj, CultureInfo.InvariantCulture);
        }

        private object MultiplyByN(float number)
        {
            var coef = OperationData.Value;
            return number * coef;
        }

        private object DivideByN(float number)
        {
            var dividend = OperationData.Value;
            return number / dividend;
        }

        private object AddN(float number)
        {
            var addend = OperationData.Value;
            return number + addend;
        }

        private object SubtractN(float number)
        {
            var subtrahend = OperationData;
            return number - subtrahend;
        }

        private object ModuloN(float number)
        {
            var modulo = OperationData.Value;
            return number % modulo;
        }

        private bool IsNumber(object value)
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

        public FormatTokenAttribute(string token, int formattingIndex = 0)
        {
            LanguageToken = token;
            FormattingIndex = formattingIndex;
        }

        public FormatTokenAttribute(string token, OperationTypeEnum opType, float opData, int formattingIndex = 0)
        {
            LanguageToken = token;
            OperationType = opType;
            OperationData = opData;
            FormattingIndex = formattingIndex;
        }
    }
}