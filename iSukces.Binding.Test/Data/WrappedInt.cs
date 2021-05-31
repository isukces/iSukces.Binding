using System;
using System.ComponentModel;
using System.Globalization;

namespace iSukces.Binding.Test.Data
{
    [TypeConverter(typeof(WrappedIntTypeConverter))]
    public struct WrappedInt
    {
        public WrappedInt(int value) { Value = value; }

        public int Value { get; }
    }

    public sealed class WrappedIntTypeConverter:TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(int) || CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(int) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is int intValue)
                return new WrappedInt(intValue);
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (value is WrappedInt wi)
            {
                if (destinationType == typeof(int))
                    return wi.Value;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
