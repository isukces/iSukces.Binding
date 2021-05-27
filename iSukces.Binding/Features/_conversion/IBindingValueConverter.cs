using System;
using System.Globalization;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public interface IBindingValueConverter
    {
        [CanBeNull]
        object Convert([CanBeNull] object value, [NotNull] Type targetType, [CanBeNull] object parameter,
            [CanBeNull] CultureInfo culture);

        [CanBeNull]
        object ConvertBack([CanBeNull] object value, [NotNull] Type targetType, [CanBeNull] object parameter,
            [CanBeNull] CultureInfo culture);
    }
}
