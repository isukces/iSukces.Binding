using System;
using System.Globalization;

namespace iSukces.Binding.Test.Data
{
    public class NumberValueConverter : IBindingValueConverter
    {
        private NumberValueConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindingSpecial)
                return value;
            if (targetType == typeof(string))
            {
                if (ValueConverterUtils.ConvertToString(ref value, parameter, culture))
                    return value;
                return value?.ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (targetType == typeof(double))
                {
                    if (double.TryParse(s, NumberStyles.Any, culture, out var doubleValue))
                        return doubleValue;
                    return BindingSpecial.Invalid;
                }

                if (targetType == typeof(int))
                {
                    if (int.TryParse(s, NumberStyles.Any, culture, out var doubleValue))
                        return doubleValue;
                    return BindingSpecial.Invalid;
                }

                if (targetType == typeof(decimal))
                {
                    if (decimal.TryParse(s, NumberStyles.Any, culture, out var doubleValue))
                        return doubleValue;
                    return BindingSpecial.Invalid;
                }
            }

            return value;
        }

        public static NumberValueConverter Instance => NumberValueConverterHolder.SingleIstance;

        public static class NumberValueConverterHolder
        {
            public static readonly NumberValueConverter SingleIstance = new NumberValueConverter();
        }
    }
}
