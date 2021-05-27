using System;

namespace iSukces.Binding
{
    internal sealed class DisposableAction : IDisposable
    {
        public DisposableAction(Action action) { _action = action; }

        ~DisposableAction() { DisposeInternal(); }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            if (_action is null)
                return;
            _action();
            _action = null;
        }

        private Action _action;
    }
}
