using System;
using System.Reflection;

namespace iSukces.Binding
{
    public static class Tools
    {
        public static Bla NeedsConversion(object value, Type t)
        {
            if (value is BindingSpecial)
                return Bla.Special;
            if (value is null)
                return t.IsClass || t.IsInterface ? Bla.Acceptable : Bla.NeedConversion;
            var tt = value.GetType();
            if (t.IsAssignableFrom(tt))
                return Bla.Acceptable;
            return Bla.NeedConversion;
        }

        public static UpdateSourceResult PropertySetValue(PropertyInfo pi, object obj, object value)
        {
            if (pi is null || obj is null)
                return UpdateSourceResult.NotSet;

            if (!pi.CanWrite)
                throw new UnableToChangeReadOnlyPropertyException("");

            if (value is BindingSpecial)
                return UpdateSourceResult.Special;


            var cat = NeedsConversion(value, pi.PropertyType);
            if (cat == Bla.Acceptable)
            {
                pi.SetValue(obj, value);
                return UpdateSourceResult.Ok;
            }
            return UpdateSourceResult.InvalidValue;
        }
    }

    public enum Bla
    {
        Special,
        Acceptable,
        NeedConversion
    }
}
