using System;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    /// <summary>
    ///     Reads and writes the properties of an object
    /// </summary>
    internal interface IObjectAccessor
    {
        /// <summary>
        ///     Returns type of accessed property. Can sometimes be null i.e. accessor in not connected to any object
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        [CanBeNull]
        Type GetPropertyType(string propertyName);

        object Read(string propertyName);
        bool TryChangeSource(object source);
        UpdateSourceResult Write(string propertyName, object value);
    }
}
