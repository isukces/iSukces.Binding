using System;
using System.Reflection;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public static class Tools
    {
        [CanBeNull]
        public static Type GetNullable(Type type)
        {
            if (type is null)
                return null;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GenericTypeArguments[0];
            return null;
        }

        public static bool IsNullable(Type type)
        {
            if (type is null)
                return false;
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static ValueConversionStatus NeedsConversion(object value, Type type)
        {
            if (value is BindingSpecial || type is null)
                return ValueConversionStatus.Special;
            if (value is null)
            {
                if (IsNullable(type))
                    return ValueConversionStatus.Acceptable;
                return type.IsClass || type.IsInterface ? ValueConversionStatus.Acceptable : ValueConversionStatus.NeedConversion;
            }

            var tt = value.GetType();
            if (type.IsAssignableFrom(tt))
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
                try
                {
                    var readed = pi.GetValue(obj);
                    return UpdateSourceResult.OkFromValue(readed);
                }
                catch
                {
                    return UpdateSourceResult.OkUnableToReadBack;
                }
            }

            return UpdateSourceResult.InvalidValue;
        }

        public static bool IsEnabled(object o)
        {
            if (BindingBootstrap.IsEnabledPredicate is null)
                return true;
            return BindingBootstrap.IsEnabledPredicate(o);
            
        }

        public static bool ShouldAssumeEmptyValue(bool ignoreDisabledControls, object target)
        {
            return !ignoreDisabledControls && !IsEnabled(target);
        }
    }

    public enum ValueConversionStatus
    {
        Special,
        Acceptable,
        NeedConversion
    }
}
