using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    internal sealed class ListerInfo
    {
        /// <summary>
        ///     Creates instance of info
        /// </summary>
        /// <param name="action">action performed when source is changed</param>
        /// <param name="typeAcceptedByListener">
        ///     declared type of source property - used by
        ///     <see cref="IBindingValueConverter">IBindingValueConverter</see>.
        ///     <see cref="IBindingValueConverter.Convert">Convert</see>
        /// </param>
        /// <param name="sourceType">
        ///     declared type of source property - used by
        ///     <see cref="IBindingValueConverter">IBindingValueConverter</see>.
        ///     <see cref="IBindingValueConverter.ConvertBack">ConvertBack</see>
        /// </param>
        /// <param name="converter">optional value converter</param>
        /// <param name="converterParameter">optional parameter for value converter</param>
        public ListerInfo(ListenerDelegate action, Type typeAcceptedByListener,
            Type sourceType,
            IBindingValueConverter converter,
            object converterParameter, CultureInfo currentCulture)
        {
            _action                 = action;
            _sourceType             = sourceType ?? typeof(object);
            _typeAcceptedByListener = typeAcceptedByListener ?? typeof(object);
            _converter              = converter;
            _converterParameter     = converterParameter;
            _currentCulture         = currentCulture ?? CultureInfo.CurrentCulture;
        }

        private object Convert(object value)
        {
            if (_converter is null) return value;
            try
            {
                value = _converter.Convert(value, _typeAcceptedByListener, _converterParameter, _currentCulture);
            }
            catch
            {
                value = BindingSpecial.Invalid;
            }

            return value;
        }

        public object ConvertBack(object value)
        {
            if (_converter is null) return value;
            try
            {
                value = _converter.ConvertBack(value, _sourceType, _converterParameter, _currentCulture);
            }
            catch
            {
                value = BindingSpecial.Invalid;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(object value, ListenerDelegateKind kind)
        {
            value = Convert(value);
            _action(value, kind);
        }

        public static Type DoesntMatter = null;

        private readonly ListenerDelegate _action;
        private readonly IBindingValueConverter _converter;
        private readonly object _converterParameter;

        [NotNull] private readonly Type _sourceType;

        [NotNull] private readonly Type _typeAcceptedByListener;

        private readonly CultureInfo _currentCulture;
    }
}
