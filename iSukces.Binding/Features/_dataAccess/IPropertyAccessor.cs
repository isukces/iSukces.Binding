using System;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    internal interface IPropertyAccessor
    {
        bool TryChangeSource(object source);
        object this[string propertyName] { get; }
        UpdateSourceResult Write(string propertyName, object value);

        /// <summary>
        /// Returns type of accessed property. Can sometimes be null i.e. accessor in not connected to any object
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        [CanBeNull]
        Type GetPropertyType(string propertyName);
    }
}
