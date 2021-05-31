using System;

namespace iSukces.Binding
{
    public interface IPropertyAccessor2 : IDisposable, IPropertyWriter
    {
        IDisposable SubscribePropertyNotification(UpdateSourceDe updateBackAction);
        Type PropertyType { get; }
    }

    public delegate UpdateSourceResult UpdateSourceDe(object value);
}
