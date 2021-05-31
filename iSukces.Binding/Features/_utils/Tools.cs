using System.Reflection;

namespace iSukces.Binding
{
    public static class Tools
    {
        public static UpdateSourceResult PropertySetValue(PropertyInfo pi, object obj, object value)
        {
            if (pi is null || obj is null)
                return UpdateSourceResult.NotSet;
            
            if (!pi.CanWrite)
                throw new UnableToChangeReadOnlyPropertyException("");
            
            if (value is BindingSpecial)
                return UpdateSourceResult.Special;

            if (value is null)
            {
                if (pi.PropertyType.IsClass)
                {
                    pi.SetValue(obj, null);
                    return UpdateSourceResult.Ok;
                }

                return UpdateSourceResult.InvalidValue;
            }

            var valueType = value.GetType();
            if (pi.PropertyType.IsAssignableFrom(valueType))
            {
                pi.SetValue(obj, value);
                return UpdateSourceResult.Ok;
            }

            return UpdateSourceResult.InvalidValue;
        }
    }
}
