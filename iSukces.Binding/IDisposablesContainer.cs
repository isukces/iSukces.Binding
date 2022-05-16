using System;

namespace iSukces.Binding
{
    public interface IDisposablesContainer : IDisposable
    {
        void AddDisposable(IDisposable disposable);
    }
}