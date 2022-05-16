using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace iSukces.Binding
{
    public sealed class DefaultValueConverter : IBindingValueConverter
    {
        private DefaultValueConverter() { }

        private static TypeConverter TryGetTypeConverter(Type type)
        {
            var at = type.GetCustomAttribute<TypeConverterAttribute>();
            if (at is null) return null;
            var ct = Type.GetType(at.ConverterTypeName);
            if (ct == null)
                throw new Exception("Unable to create " + at.ConverterTypeName);
            return (TypeConverter)Activator.CreateInstance(ct);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindingSpecial)
                return value;
            if (value is null)
                return null;
            var currentType = value.GetType();
            if (targetType.IsAssignableFrom(currentType))
                return value;

            #region Convertible

            {
                if (value is IConvertible x)
                {
                    if (targetType == typeof(byte)) return x.ToByte(culture);
                    if (targetType == typeof(short)) return x.ToInt16(culture);
                    if (targetType == typeof(int)) return x.ToInt32(culture);
                    if (targetType == typeof(long)) return x.ToInt64(culture);

                    if (targetType == typeof(sbyte)) return x.ToSByte(culture);
                    if (targetType == typeof(ushort)) return x.ToUInt16(culture);
                    if (targetType == typeof(uint)) return x.ToUInt32(culture);
                    if (targetType == typeof(ulong)) return x.ToUInt64(culture);

                    if (targetType == typeof(double)) return x.ToDouble(culture);
                    if (targetType == typeof(decimal)) return x.ToDecimal(culture);
                    if (targetType == typeof(float)) return x.ToSingle(culture);

                    if (targetType == typeof(char)) return x.ToChar(culture);
                    if (targetType == typeof(bool)) return x.ToBoolean(culture);
                    if (targetType == typeof(DateTime)) return x.ToDateTime(culture);
                }
            }

            #endregion

            #region Type converter

            {
                var co = TryGetTypeConverter(currentType);
                if (co is not null)
                {
                    if (co.CanConvertTo(targetType))
                    {
                        var c = co.ConvertTo(null, culture, value, targetType);
                        return c;
                    }
                }
            }
            {
                var co = TryGetTypeConverter(targetType);
                if (co is not null)
                {
                    if (co.CanConvertFrom(currentType))
                    {
                        var c = co.ConvertFrom(null!, culture ?? CultureInfo.CurrentCulture, value);
                        return c;
                    }
                }
            }

            #endregion

            #region To string with optional IFormattable

            if (targetType == typeof(string))
            {
                if (value is IFormattable formattable)
                {
                    if (parameter is string format)
                        return formattable.ToString(format, culture);
                    return formattable.ToString(null, culture);
                }

                return value.ToString();
            }

            #endregion

            #region Integer

            if (targetType == typeof(int))
            {
                if (ValueConverterUtils.TryConvertToInt(value, out var intValue))
                    return intValue;
            }

            #endregion

            return BindingSpecial.Invalid; // give up
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindingSpecial)
                return value;
            if (value is null)
                return null;
            var currentType = value.GetType();
            if (targetType.IsAssignableFrom(currentType))
                return value;


            {
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if (value is string txt && string.IsNullOrWhiteSpace(txt))
                        return null;
                    var genericArgument = targetType.GetGenericArguments()[0];
                    return ConvertBack(value, genericArgument, parameter, culture);

                }
            }

            #region Type converter

            {
                var co = TryGetTypeConverter(targetType);
                if (co is not null)
                {
                    if (co.CanConvertFrom(currentType))
                    {
                        var c = co.ConvertFrom(null!, culture ?? CultureInfo.CurrentCulture, value);
                        return c;
                    }
                }
            }
            {
                var co = TryGetTypeConverter(currentType);
                if (co is not null)
                {
                    if (co.CanConvertTo(targetType))
                    {
                        var c = co.ConvertTo(null!, culture ?? CultureInfo.CurrentCulture, value, targetType);
                        return c;
                    }
                }
            }

            #endregion

            if (value is IConvertible)
            {
                try
                {
                    var tt = System.Convert.ChangeType(value, targetType, culture);
                    return tt; // IConvertible
                }
                catch (InvalidCastException e)
                {
                    return BindingSpecial.Invalid;
                }
            }

            return BindingSpecial.Invalid; // give up
        }

        public static DefaultValueConverter Instance => DefaultValueConverterHolder.SingleIstance;

        public static class DefaultValueConverterHolder
        {
            public static readonly DefaultValueConverter SingleIstance = new DefaultValueConverter();
        }
    }
}
