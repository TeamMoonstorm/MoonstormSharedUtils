using BepInEx.Configuration;
using HG.Reflection;
using MSU.Config;
using System;
using System.Globalization;
using System.Reflection;

namespace MSU
{
    /// <summary>
    /// The FormatTokenAttribute is a <see cref="SearchableAttribute"/> that can be used for obtaning values for a Token, which then said token will be formated using said values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class FormatTokenAttribute : SearchableAttribute
    {
        /// <summary>
        /// The language token to be formatted
        /// </summary>
        public string languageToken { get; private set; }

        /// <summary>
        /// A type of operation to apply to the value stored in the field/property this attribute is attached to.
        /// </summary>
        public OperationTypeEnum? operationType { get; private set; }

        /// <summary>
        /// A number to use to modify the value stored in the field/property this attribute is attached to. Only relevant if there's a value in <see cref="operationType"/>
        /// </summary>
        public float? operationData { get; private set; }

        /// <summary>
        /// The index used during the formatting process
        /// </summary>ob
        /// 
        public int formattingIndex { get; private set; }

        private object _cachedFormattingValue;

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
            else
            {
                throw new InvalidOperationException("FormatTokenAttribute is only valid in Fields and Properties");
            }

            Type valueType = value.GetType();

            if (valueType.IsSubclassOf(typeof(ConfiguredVariable)))
            {
                PropertyInfo configEntryBase = valueType.GetProperty(nameof(ConfiguredVariable.configEntryBase), BindingFlags.Public | BindingFlags.Instance);
                var cfg = (ConfigEntryBase)configEntryBase.GetMethod?.Invoke(value, null);
                value = cfg.BoxedValue;
            }

            if (value == null)
                return string.Empty;

            if (IsNumber(value))
            {
                if (operationType.HasValue && operationData.HasValue)
                {
                    switch (operationType.Value)
                    {
                        case OperationTypeEnum.MultiplyByN:
                            value = MultiplyByN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.DivideByN:
                            value = DivideByN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.AddN:
                            value = AddN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.ModuloN:
                            value = ModuloN(CastToFloat(value));
                            break;
                        case OperationTypeEnum.SubtractN:
                            value = SubtractN(CastToFloat(value));
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
            var coef = operationData.Value;
            return number * coef;
        }

        private object DivideByN(float number)
        {
            var dividend = operationData.Value;
            return number / dividend;
        }

        private object AddN(float number)
        {
            var addend = operationData.Value;
            return number + addend;
        }

        private object SubtractN(float number)
        {
            var subtrahend = operationData;
            return number - subtrahend;
        }

        private object ModuloN(float number)
        {
            var modulo = operationData.Value;
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

        /// <summary>
        /// Constructor for the FormatTokenAttribute
        /// </summary>
        /// <param name="token">The token to format</param>
        /// <param name="formattingIndex">The index to format</param>
        public FormatTokenAttribute(string token, int formattingIndex = 0)
        {
            languageToken = token;
            this.formattingIndex = formattingIndex;
        }

        /// <summary>
        /// Constructor for the FormatTokenAttribute
        /// </summary>
        /// <param name="token">The token to format</param>
        /// <param name="opType">A type of operation that'll be applied to the field/property's value before formatting occurs</param>
        /// <param name="opData">The number used during the operation that'll be applied to the field/property's value before formatting occurs</param>
        /// <param name="formattingIndex">The index to format</param>
        public FormatTokenAttribute(string token, OperationTypeEnum opType, float opData, int formattingIndex = 0)
        {
            languageToken = token;
            operationType = opType;
            operationData = opData;
            this.formattingIndex = formattingIndex;
        }

        /// <summary>
        /// Represents basic arithmetic operations used in the <see cref="FormatTokenAttribute"/>
        /// </summary>
        public enum OperationTypeEnum : int
        {
            /// <summary>
            /// Represents a Division (/) operator
            /// </summary>
            DivideByN,
            /// <summary>
            /// Represents a Multiplication (*) operator
            /// </summary>
            MultiplyByN,
            /// <summary>
            /// Represents an Addition (+) operator
            /// </summary>
            AddN,
            /// <summary>
            /// Represents a Subtraction (-) operator
            /// </summary>
            SubtractN,
            /// <summary>
            /// Represents a Modulo (%) operator
            /// </summary>
            ModuloN,
        }
    }
}