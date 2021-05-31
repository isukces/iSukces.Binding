using System;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public interface IPropertyInfoProviderRegistry
    {
        [NotNull]
        IPropertyInfo FindProvider(Type type, [NotNull] string propertyName);
        void RegisterProvider(IPropertyInfoProvider provider);
    }
}
