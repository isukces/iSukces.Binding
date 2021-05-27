using System;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public class BindingBuilder
    {
        internal BindingBuilder(BindingValueWrapper wrapper) { _wrapper = wrapper; }


        public IDisposable CreateListener(ListenerDelegate listener)
        {
            return CreateListener(listener, typeof(object));
        }

        public IDisposable CreateListener(ListenerDelegate listener, Type expectedType)
        {
            var disposables = CreateListener(listener, expectedType, null);
            return disposables.MainDisposing;
        }


        private DisposableHolder CreateListener(ListenerDelegate listener, Type expectedType,
            Func<Action, ListerInfo, BindingValueWrapper, IDisposable> factory)
        {
            var disposables = new DisposableHolder();
            var wrapper     = _wrapper.CreateAccessor(Path);
            var info        = new ListerInfo(listener, expectedType, Converter);
            disposables.RemoveFromListerer = wrapper.AddListenerAction(info);

            void DisposableAction()
            {
                if (disposables.MainDisposing is null)
                    return;
                _wrapper.BindingManager.RemoveDisposable(disposables.MainDisposing);
                disposables.RemoveFromListerer?.Dispose();
            }

            if (factory is null)
                disposables.MainDisposing = new DisposableAction(DisposableAction);
            else
                disposables.MainDisposing = factory(DisposableAction, info, wrapper);
            _wrapper.BindingManager.AddDisposable(disposables.MainDisposing);
            return disposables;
        }

        public ITwoWayBinding CreateTwoWayBinding<T>(ListenerDelegate listener)
        {
            return CreateTwoWayBinding(listener, typeof(T));
        }

        public ITwoWayBinding CreateTwoWayBinding(ListenerDelegate listener, Type expectedType)
        {
            var disposables = CreateListener(listener, expectedType, (action, info, wrapper) =>
            {
                var result = new TwoWayBinding(action, obj =>
                {
                   return wrapper.UpdateSource(obj, info);
                });
                return result;
            });
            return (ITwoWayBinding)disposables.MainDisposing;
        }

        public BindingBuilder WithPath(string path)
        {
            Path = path;
            return this;
        }

        public string                 Path      { get; set; }
        public IBindingValueConverter Converter { get; set; }
        private readonly BindingValueWrapper _wrapper;


        /// <summary>
        ///     Holds references to disposable objects. Its required to avoid
        ///     'Access to modified captured variable'
        /// </summary>
        sealed class DisposableHolder
        {
            [CanBeNull]
            public IDisposable RemoveFromListerer { get; set; }

            public IDisposable MainDisposing { get; set; }
        }
    }
}
