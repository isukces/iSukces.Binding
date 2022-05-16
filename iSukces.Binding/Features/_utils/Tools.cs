using System;
using System.Reflection;

namespace iSukces.Binding
{
    public static class Tools
    {
        public static ValueConversionStatus NeedsConversion(object value, Type t)
        {
            if (value is BindingSpecial || t is null)
                return ValueConversionStatus.Special;
            if (value is null)
                return t.IsClass || t.IsInterface ? ValueConversionStatus.Acceptable : ValueConversionStatus.NeedConversion;
            var tt = value.GetType();
            if (t.IsAssignableFrom(tt))
                return ValueConversionStatus.Acceptable;
            return ValueConversionStatus.NeedConversion;
        }

        public static UpdateSourceResult PropertySetValue(PropertyInfo pi, object obj, object value)
        {
            if (pi is null || obj is null)
                return UpdateSourceResult.NotSet;

            if (!pi.CanWrite)
                throw new UnableToChangeReadOnlyPropertyException(pi.Name + " of " + pi.ReflectedType);

            if (value is BindingSpecial)
                return UpdateSourceResult.Special;


            var cat = NeedsConversion(value, pi.PropertyType);
            if (cat == ValueConversionStatus.Acceptable)
            {
                pi.SetValue(obj, value);
                return UpdateSourceResult.Ok;
            }
            return UpdateSourceResult.InvalidValue;
        }
    }

    public enum ValueConversionStatus
    {
        Special,
        Acceptable,
        NeedConversion
    }
}
