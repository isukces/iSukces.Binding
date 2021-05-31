using System;
using System.ComponentModel;
using System.Reflection;

namespace iSukces.Binding
{
    public sealed class NotifyPropertyChangedPropertyInfoProvider : IPropertyInfoProvider
    {
        public IPropertyInfo GetForProperty(Type type, string propertyName, UpdateSourceTrigger trigger)
        {
            var p        = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            var features = BindingFeatures.None;
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                features = BindingFeatures.OnPropertyChanged;
            return new ReflectionPropertyInfo(p, -100, features);
        }
    }
}
