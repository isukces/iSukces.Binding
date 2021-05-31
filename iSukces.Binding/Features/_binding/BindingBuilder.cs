using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public partial class BindingBuilder
    {
        internal BindingBuilder(BindingValueWrapper wrapper) { _wrapper = wrapper; }


        public IDisposable BindTo<TTarget>(TTarget target,
            Expression<Func<TTarget, object>> propertyExpression)
        {
            string propertyName = ExpressionTools.GetBindingPath(propertyExpression);
            return new Builder(this).Create(target, propertyName);
        }

        public IDisposable BindTo<TTarget>(TTarget target,
            Expression<Func<TTarget, object>> propertyExpression, BindingMode mode)
        {
            var propertyName = ExpressionTools.GetBindingPath(propertyExpression);
            return new Builder(this).Create(target, propertyName, mode);
        }

        public IDisposable BindTo<TTarget>(TTarget target, string propertyName)
        {
            return new Builder(this).Create(target, propertyName);
        }

        public IDisposable BindToOneWay<TTarget>(TTarget target,
            Expression<Func<TTarget, object>> propertyExpression)
        {
            Mode = BindingMode.OneWay;
            var propertyName = ExpressionTools.GetBindingPath(propertyExpression);
            return new Builder(this).Create(target, propertyName);
        }

        public IDisposable BindToTwoWay<TTarget>(TTarget target,
            Expression<Func<TTarget, object>> propertyExpression)
        {
            Mode = BindingMode.TwoWay;
            var propertyName = ExpressionTools.GetBindingPath(propertyExpression);
            return new Builder(this).Create(target, propertyName);
        }


        public IDisposable CreateListener(ListenerDelegate listener)
        {
            return new Builder(this).CreateListener(listener, ListerInfo.DoesntMatter, null);
        }

        public IDisposable CreateListener(ListenerDelegate listener, Type typeAcceptedByListener)
        {
            var disposable = new Builder(this).CreateListener(listener, typeAcceptedByListener, null);
            return disposable;
        }

        private BindingManager GetBindingManager() { return _wrapper.BindingManager; }


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

        public BindingBuilder WithMode(BindingMode mode)
        {
            Mode = mode;
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
        public BindingMode            Mode               { get; set; }

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
