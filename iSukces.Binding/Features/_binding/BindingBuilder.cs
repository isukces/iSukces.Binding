using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
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
            var builder = new Builder(this);
            return builder.Create(target, propertyName);
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

        public BindingBuilder WithStringFormat(string format)
        {
            StringFormat = format;
            return this;
        }

        public BindingBuilder WithValidator(BindingValidator validator)
        {
            Validators.Add(validator);
            return this;
        }

        public BindingBuilder WithValidatorsFromAttributes()
        {
            PropertyInfo GetProperty()
            {
                if (string.IsNullOrEmpty(Path))
                    return null;
                var currentType = _wrapper.Source?.GetType();
                if (currentType is null)
                    return null;
                var items = Path.Split('.');
                for (var index = 0; index < items.Length; index++)
                {
                    var propertyName = items[index];
                    var property = currentType.GetProperty(
                        propertyName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (property is null || index == items.Length - 1)
                        return property;
                    currentType = property.PropertyType;
                }

                return null;
            }

            var prop = GetProperty();
            if (prop is null)
                return this;

            var minLengthAttribute = prop.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttribute != null)
                Validators.Add(new MinStringLengthBindingValidator(minLengthAttribute.Length));

            return this;
        }

        public IList<BindingValidator> Validators { get; } = new List<BindingValidator>();

        public string                 Path               { get; set; }
        public IBindingValueConverter Converter          { get; set; }
        public object                 ConverterParameter { get; set; }
        public CultureInfo            CultureInfo        { get; set; }
        public Dispatcher             ListenerDispatcher { get; set; }
        public BindingMode            Mode               { get; set; }

        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

        public string StringFormat { get; set; }

        public object ListenerTag { get; set; }

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

        public ITwoWayBinding CreateTwoWayBinding<T>(ListenerDelegate listener)
        {
            return new Builder(this).CreateTwoWayBinding(listener, typeof(T));
        }


    }

    [Flags]
    public enum UpdateSourceTriggerFlags
    {
        None,
        PropertyChanged = 1,
        LostFocus = 2,
        Any = 3
    }

    public enum UpdateSourceTrigger
    {
        Default,
        PropertyChanged,
        LostFocus
    }
}
