using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
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
        /// <param name="converter">optional value converter</param>
        /// <param name="converterParameter">optional parameter for value converter</param>
        /// <param name="currentCulture"></param>
        /// <param name="listenerDispatcher"></param>
        public ListerInfo(ListenerDelegate action, Type typeAcceptedByListener,
            IBindingValueConverter converter,
            object converterParameter, CultureInfo currentCulture,
            Dispatcher listenerDispatcher)
        {
            _action                 = action;
            _typeAcceptedByListener = typeAcceptedByListener ?? typeof(object);
            _converter              = converter;
            _converterParameter     = converterParameter;
            _listenerDispatcher     = listenerDispatcher;
            _currentCulture         = currentCulture ?? CultureInfo.CurrentCulture;
        }

        private bool Convert(ref object value)
        {
            if (_converter is null)
                return false;
            try
            {
                value = _converter.Convert(value, _typeAcceptedByListener, _converterParameter, _currentCulture);
            }
            catch
            {
                value = BindingSpecial.Invalid;
            }

            return true;
        }

        public object ConvertBack(object value, Type sourceType)
        {
            if (_converter is null) return value;
            try
            {
                value = _converter.ConvertBack(value, sourceType, _converterParameter, _currentCulture);
            }
            catch
            {
                value = BindingSpecial.Invalid;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(IValueInfo info)
        {
            var value = info.Value;
            if (Convert(ref value))
            {
                info = PrvValueInfo.Make(info, value);
            }

            if (_listenerDispatcher is null || _listenerDispatcher.CheckAccess())
                _action(info);
            else
            {
                void DispatcherMethod()
                {
                    _action(info);
                }

                try
                {
                    _listenerDispatcher.Invoke(DispatcherMethod);
                }
                catch (TaskCanceledException)
                {
                    
                }
                //_listenerDispatcher.BeginInvoke((Action)DispatcherMethod, DispatcherPriority.Background);
            }
        }

        public static readonly Type DoesntMatter = null;

        private readonly ListenerDelegate _action;
        private readonly IBindingValueConverter _converter;
        private readonly object _converterParameter;

        private readonly CultureInfo _currentCulture;
        private readonly Dispatcher _listenerDispatcher;
        
        [NotNull] private readonly Type _typeAcceptedByListener;
    }
}
