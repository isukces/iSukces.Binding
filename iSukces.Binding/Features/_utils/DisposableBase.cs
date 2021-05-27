using System;

namespace iSukces.Binding
{
    public class DisposableBase : IDisposable
    {
        public DisposableBase(DisposingStateBehaviour disposingStateBehaviour)
        {
            _disposingStateBehaviour = disposingStateBehaviour;
        }

        ~DisposableBase()
        {
            if (_disposingState != DisposingState.Alive)
                return;
            _disposingState = DisposingState.Disposing;
            DisposeInternal(false);
            _disposingState = DisposingState.Disposed;
        }

        public void Dispose()
        {
            if (_disposingState != DisposingState.Alive)
            {
                if ((_disposingStateBehaviour & DisposingStateBehaviour.Throw) != 0)
                    throw new ObjectDisposedException("Already disposed");
                return;
            }

            _disposingState = DisposingState.Disposing;
            DisposeInternal(true);
            _disposingState = DisposingState.Disposed;
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
            if (_disposingState != DisposingState.Alive)
                throw new ObjectDisposedException("Already disposed");
        }

        protected DisposingState _disposingState { get; private set; }

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
