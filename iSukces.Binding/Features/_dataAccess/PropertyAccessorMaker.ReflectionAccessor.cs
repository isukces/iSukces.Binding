﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace iSukces.Binding
{
    internal partial class PropertyAccessorMaker
    {
        internal class ReflectionAccessor : IPropertyAccessor
        {
            public ReflectionAccessor(object source)
            {
                _source = source;
                _type   = source?.GetType();
            }

            public bool TryChangeSource(object source)
            {
                if (EmptyAccessor.AcceptsSource(source))
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

            public object this[string propertyName]
            {
                get
                {
                    if (_type is null)
                        return BindingSpecial.NotSet;
                    if (!_byProperty.TryGetValue(propertyName, out var propertyInfo))
                    {
                        _byProperty[propertyName] = propertyInfo =
                            _type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    }

                    if (propertyInfo is null)
                        return BindingSpecial.NotSet;
                    return propertyInfo.GetValue(_source);
                }
                set => throw new NotImplementedException();
            }

            private readonly Dictionary<string, PropertyInfo> _byProperty = new();
            private object _source;

            private Type _type;
        }
    }
}