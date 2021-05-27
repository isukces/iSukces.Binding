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
    }
}
