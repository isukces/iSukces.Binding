using System;

namespace iSukces.Binding
{
    public class BindingBuilder
    {
        internal BindingBuilder(BindingValueWrapper wrapper) { _wrapper = wrapper; }

        public IDisposable CreateListener(ListenerDelegate action)
        {
            var disposables = new IDisposable[2];
            var wrapper     = _wrapper.CreateAccessor(Path);
            // disposables[1] can be null
            disposables[1] = wrapper.AddListenerAction(action);
            disposables[0] = new DisposableAction(() =>
            {
                if (disposables[0] is null)
                    return;
                _wrapper.BindingManager.RemoveDisposable(disposables[0]);
                disposables[1]?.Dispose();
            });
            _wrapper.BindingManager.AddDisposable(disposables[0]);
            return disposables[0];
        }

        public BindingBuilder WithPath(string path)
        {
            Path = path;
            return this;
        }

        public string Path { get; set; }
        private readonly BindingValueWrapper _wrapper;
    }
}
