using System;
using System.Globalization;

namespace iSukces.Binding.Test.Data
{
    public class IntValueConverter : IBindingValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
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
