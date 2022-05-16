using System;

namespace iSukces.Binding
{
    public abstract class DisposableBase : IDisposable
    {
        protected DisposableBase(DisposingStateBehaviour disposingStateBehaviour)
        {
            _disposingStateBehaviour = disposingStateBehaviour;
        }

        ~DisposableBase()
        {
            if (ObjectDisposingState != DisposingState.Alive)
                return;
            ObjectDisposingState = DisposingState.Disposing;
            DisposeInternal(false);
            ObjectDisposingState = DisposingState.Disposed;
        }

        public void Dispose()
        {
            if (ObjectDisposingState != DisposingState.Alive)
            {
                if ((_disposingStateBehaviour & DisposingStateBehaviour.Throw) != 0)
                    throw new ObjectDisposedException("Already disposed");
                return;
            }

            ObjectDisposingState = DisposingState.Disposing;
            DisposeInternal(true);
            ObjectDisposingState = DisposingState.Disposed;
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeInternal(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources here
            }
            // Release unmanaged resources here
        }

        protected void ThrowIfDisposed()
        {
            if (ObjectDisposingState != DisposingState.Alive)
                throw new ObjectDisposedException("Already disposed");
        }

        public DisposingState ObjectDisposingState { get; private set; }

        private readonly DisposingStateBehaviour _disposingStateBehaviour;
    }

    public enum DisposingState
    {
        Alive,
        Disposing,
        Disposed
    }

    [Flags]
    public enum DisposingStateBehaviour
    {
        None = 0,
        Throw = 1
    }
}
