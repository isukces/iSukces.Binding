using System;
using System.Globalization;

namespace iSukces.Binding
{
    public static class ValueConverterUtils
    {
        public static bool ConvertToString(ref object value, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case BindingSpecial:
                    return false;
                case string:
                case null:
                    return true;
            }

            if (parameter is string format)
            {
                switch (value)
                {
                    case int intValue:
                        value = intValue.ToString(format, culture);
                        return true;
                    case uint uintValue:
                        value = uintValue.ToString(format, culture);
                        return true;
                    case long longValue:
                        value = longValue.ToString(format, culture);
                        return true;
                    case ulong ulongValue:
                        value = ulongValue.ToString(format, culture);
                        return true;
                    case byte byteValue:
                        value = byteValue.ToString(format, culture);
                        return true;
                    case sbyte sbyteValue:
                        value = sbyteValue.ToString(format, culture);
                        return true;
                    case double doubleValue:
                        value = doubleValue.ToString(format, culture);
                        return true;
                    case decimal decimalValue:
                        value = decimalValue.ToString(format, culture);
                        return true;
                    case float floatValue:
                        value = floatValue.ToString(format, culture);
                        return true;
                    case Guid guidValue:
                        value = guidValue.ToString(format, culture);
                        return true;
                    case DateTime dateTimeValue:
                        value = dateTimeValue.ToString(format, culture);
                        return true;
                    case DateTimeOffset dateTimeOffsetValue:
                        value = dateTimeOffsetValue.ToString(format, culture);
                        return true;
                    case TimeSpan timeSpanValue:
                        value = timeSpanValue.ToString(format, culture);
                        return true;
                }
            }

            if (value is IConvertible convertible)
            {
                value = convertible.ToString(culture);
                return true;
            }

            return false;
        }

        public static bool TryConvertToInt(object o, out int value)
        {
            switch (o)
            {
                case int intValue:
                    value = intValue; return true;
                case uint uintValue and <= int.MaxValue:
                    value = (int)uintValue;
                    return true;
                case long longValue and >= int.MinValue and <= int.MaxValue:
                    value = (int)longValue;
                    return true;
                case ulong ulongValue and <= int.MaxValue:
                    value = (int)ulongValue;
                    return true;
                case byte byteValue:
                    value = byteValue;
                    return true;
                case sbyte sbyteValue:
                    value = sbyteValue; 
                    return true;
                case short shortValue:
                    value = shortValue;
                    return true;
                case ushort ushortValue:
                    value = ushortValue;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }
    }
}
