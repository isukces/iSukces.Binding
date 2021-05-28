using System;
using System.Globalization;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public class BindingBuilder
    {
        internal BindingBuilder(BindingValueWrapper wrapper) { _wrapper = wrapper; }


        public IDisposable CreateListener(ListenerDelegate listener)
        {
            return CreateListener(listener, ListerInfo.DoesntMatter);
        }
        
        public IDisposable CreateListener(ListenerDelegate listener, Type typeAcceptedByListener)
        {
            var disposables = CreateListener(listener, typeAcceptedByListener, null);
            return disposables.MainDisposing;
        }

        private DisposableHolder CreateListener(ListenerDelegate listener,
            Type typeAcceptedByListener,
            Func<Action, ListerInfo, BindingValueWrapper, IDisposable> factory)
        {
            var disposables = new DisposableHolder();
            var wrapper     = _wrapper.CreateAccessor(Path);
            var info = new ListerInfo(listener, typeAcceptedByListener, Converter, ConverterParameter,
                CultureInfo, ListenerDispatcher);
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
        

        public ITwoWayBinding CreateTwoWayBinding<TListener>(ListenerDelegate listener)
        {
            return CreateTwoWayBinding(listener, typeof(TListener));
        }

        public ITwoWayBinding CreateTwoWayBinding(ListenerDelegate listener, Type typeAcceptedByListener)
        {
            var disposables = CreateListener(listener, typeAcceptedByListener, (action, info, wrapper) =>
            {
                var result = new TwoWayBinding(action, obj =>
                {
                    return wrapper.UpdateSource(obj, info);
                });
                return result;
            });
            return (ITwoWayBinding)disposables.MainDisposing;
        }

        public BindingBuilder WithConverter(IBindingValueConverter converter)
        {
            Converter = converter;
            return this;
        }

        public BindingBuilder WithConverter(IBindingValueConverter converter, object converterParameter)
        {
            Converter          = converter;
            ConverterParameter = converterParameter;
            return this;
        }

        public BindingBuilder WithListenerDispatcher(Dispatcher dispatcher = null)
        {
            ListenerDispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
            return this;
        }

        public BindingBuilder WithPath(string path)
        {
            Path = path;
            return this;
        }


        public string                 Path               { get; set; }
        public IBindingValueConverter Converter          { get; set; }
        public object                 ConverterParameter { get; set; }
        public CultureInfo            CultureInfo        { get; set; }
        public Dispatcher             ListenerDispatcher { get; set; }

        private readonly BindingValueWrapper _wrapper;


        /// <summary>
        ///     Holds references to disposable objects. Its required to avoid
        ///     'Access to modified captured variable'
        /// </summary>
        private sealed class DisposableHolder
        {
            [CanBeNull]
            public IDisposable RemoveFromListerer { get; set; }

            public IDisposable MainDisposing { get; set; }
        }
    }
}
