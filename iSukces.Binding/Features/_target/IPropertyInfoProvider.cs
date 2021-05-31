using System;

namespace iSukces.Binding
{
    public interface IPropertyInfoProvider
    {
        IPropertyInfo GetForProperty(Type type, string propertyName);
    }
}
