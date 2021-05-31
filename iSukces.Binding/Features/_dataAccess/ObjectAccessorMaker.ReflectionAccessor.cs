using System;
using System.Collections.Generic;
using System.Reflection;

namespace iSukces.Binding
{
    internal partial class ObjectAccessorMaker
    {
        internal class ReflectionAccessor : IObjectAccessor
        {
            public ReflectionAccessor(object source)
            {
                _source = source;
                _type   = source?.GetType();
            }

            public Type GetPropertyType(string propertyName)
            {
                if (_type is null)
                    return null;
                var propertyInfo = SurePropertyInfo(propertyName);
                return propertyInfo?.PropertyType;
            }

            public object Read(string propertyName)
            {
                var propertyInfo = SurePropertyInfo(propertyName);
                if (propertyInfo is null)
                    return BindingSpecial.NotSet;
                return propertyInfo.GetValue(_source);
            }

            private PropertyInfo SurePropertyInfo(string propertyName)
            {
                if (_type is null)
                    return null;
                if (!_byProperty.TryGetValue(propertyName, out var propertyInfo))
                {
                    _byProperty[propertyName] = propertyInfo =
                        _type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                }

                return propertyInfo;
            }

            public bool TryChangeSource(object source)
            {
                if (EmptyAccessor.AcceptsSource(source))
                    return false;
                if (Unbound.AcceptsSource(source))
                    return false;
                _source = source;
                if (source is null)
                {
                    _type = null;
                    _byProperty.Clear();
                    return true;
                }

                var newType = source.GetType();
                if (newType == _type)
                    return true;
                _type = newType;
                _byProperty.Clear();
                return true;
            }

            public UpdateSourceResult Write(string propertyName, object value)
            {
                var propertyInfo = SurePropertyInfo(propertyName);
                return Tools.PropertySetValue(propertyInfo, _source, value);
            }

            private readonly Dictionary<string, PropertyInfo> _byProperty = new();
            private object _source;
            private Type _type;
        }
    }
}
