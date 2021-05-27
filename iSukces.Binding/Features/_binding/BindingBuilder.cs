using System;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public class BindingBuilder
    {
        internal BindingBuilder(BindingValueWrapper wrapper) { _wrapper = wrapper; }


        public IDisposable CreateListener(ListenerDelegate listener)
        {
            return CreateListener(listener, ListerInfo.DoesntMatter, ListerInfo.DoesntMatter);
        }

        public IDisposable CreateListener(ListenerDelegate listener, Type typeAcceptedByListener)
        {
            return CreateListener(listener, typeAcceptedByListener, ListerInfo.DoesntMatter);
        }

        public IDisposable CreateListener(ListenerDelegate listener, Type typeAcceptedByListener, Type sourceType)
        {
            var disposables = CreateListener(listener, typeAcceptedByListener, sourceType, null);
            return disposables.MainDisposing;
        }

        private DisposableHolder CreateListener(ListenerDelegate listener,
            Type typeAcceptedByListener, Type sourceType,
            Func<Action, ListerInfo, BindingValueWrapper, IDisposable> factory)
        {
            var disposables = new DisposableHolder();
            var wrapper = _wrapper.CreateAccessor(Path);
            var info = new ListerInfo(listener, typeAcceptedByListener, sourceType, Converter, CnverterParameter);
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

        public ITwoWayBinding CreateTwoWayBinding<TSource>(ListenerDelegate listener, Type typeAcceptedByListener = null)
        {
            return CreateTwoWayBinding(listener, typeAcceptedByListener, typeof(TSource));
        }
        
        public ITwoWayBinding CreateTwoWayBinding<TSource, TListener>(ListenerDelegate listener)
        {
            return CreateTwoWayBinding(listener, typeof(TListener), typeof(TSource));
        }

        public ITwoWayBinding CreateTwoWayBinding(ListenerDelegate listener, Type typeAcceptedByListener,
            Type sourceType)
        {
            var disposables = CreateListener(listener, typeAcceptedByListener, sourceType, (action, info, wrapper) =>
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

        public object CnverterParameter { get; set; }

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
