using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public sealed class SimplePropertyInfoProviderRegistry : IPropertyInfoProviderRegistry
    {
        public SimplePropertyInfoProviderRegistry(bool registerSimpleProviders)
        {
            if (!registerSimpleProviders) return;
            RegisterProvider(new NotifyPropertyChangedPropertyInfoProvider());
        }

        public IPropertyInfo FindProvider(Type type, string propertyName)
        {
            if (type == null)
                return new FakePropertyInfo();
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var key = new Key(type, propertyName);
            if (_cache.TryGetValue(key, out var value))
                return value;
            for (var index = 0; index < _list.Count; index++)
            {
                var provider  = _list[index];
                var candidate = provider.GetForProperty(type, propertyName);
                if (candidate is null)
                    continue;
                if (value is null || candidate.Priority > value.Priority)
                    value = candidate;
            }

            if (value is null)
                value = new FakePropertyInfo();
            return _cache[key] = value;
        }

        public void RegisterProvider(IPropertyInfoProvider provider)
        {
            _list.Add(provider);
            _cache.Clear();
        }

        private readonly Dictionary<Key, IPropertyInfo> _cache = new();

        private readonly List<IPropertyInfoProvider> _list = new();

        [ImmutableObject(true)]
        private class Key : IEquatable<Key>
        {
            public Key([NotNull] Type type, [NotNull] string propertyName)
            {
                _type         = type ?? throw new ArgumentNullException(nameof(type));
                _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            }

            public static bool operator ==(Key left, Key right) { return Equals(left, right); }
            public static bool operator !=(Key left, Key right) { return !Equals(left, right); }

            public bool Equals(Key other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _type == other._type && _propertyName == other._propertyName;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Key)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_type.GetHashCode() * 397) ^ _propertyName.GetHashCode();
                }
            }

            public override string ToString() { return $"Type={_type}, PropertyName={_propertyName}"; }

            private readonly string _propertyName;

            private readonly Type _type;
        }
    }
}
