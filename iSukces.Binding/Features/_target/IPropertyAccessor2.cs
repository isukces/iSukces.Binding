using System;

namespace iSukces.Binding
{
    public interface IPropertyAccessor2 : IDisposable, IPropertyWriter
    {
        IPropertyUpdateSession SubscribePropertyNotification(UpdateSourceDe updateBackAction);
        Type PropertyType { get; }
    }

    public delegate UpdateSourceResult UpdateSourceDe(object value);

    public interface IPropertyUpdateSession : IDisposable
    {
        void ForceUpdate(bool ignoreDisabledControls);
    }
}
