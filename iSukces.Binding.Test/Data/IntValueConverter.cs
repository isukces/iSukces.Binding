using System;
using System.Globalization;

namespace iSukces.Binding.Test.Data
{
    public class IntValueConverter : IBindingValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;
            if (value is BindingSpecial)
                return value;
            if (targetType == typeof(string))
            {
                if (value is IConvertible i)
                {
                    return i.ToString(culture);
                }

                return value.ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (targetType == typeof(int))
                {
                    if (int.TryParse(s, NumberStyles.Any, culture, out var intValue))
                        return intValue;
                    return BindingSpecial.Invalid;
                }
            }

            return value;
        }
    }
}
