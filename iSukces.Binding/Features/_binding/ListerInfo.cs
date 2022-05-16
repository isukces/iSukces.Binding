using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public interface IListerInfo
    {
        [NotNull] Type TypeAcceptedByListener { get; }
        object         Tag                    { get; } 
    }
    internal sealed class ListerInfo:IListerInfo
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
        /// <param name="stringFormat"></param>
        /// <param name="tag"></param>
        public ListerInfo(ListenerDelegate action, Type typeAcceptedByListener,
            IBindingValueConverter converter,
            object converterParameter,
            CultureInfo currentCulture,
            Dispatcher listenerDispatcher,
            string stringFormat, 
            object tag)
        {
            _action                = action;
            TypeAcceptedByListener = typeAcceptedByListener ?? typeof(object);
            _converter             = converter;
            _converterParameter    = converterParameter;
            _listenerDispatcher    = listenerDispatcher;
            _stringFormat          = stringFormat;
            _currentCulture        = currentCulture ?? CultureInfo.CurrentCulture;
            Tag                    = tag;
        }

        private bool Convert(ref object value)
        {
            void ConvertWithDefault(ref object value2)
            {
                if (!string.IsNullOrEmpty(this._stringFormat))
                {
                    if (TypeAcceptedByListener == typeof(string))
                    {
                        if (value2 is IFormattable d)
                        {
                            value2 = d.ToString(_stringFormat, _currentCulture);
                            return;
                        }
                    }

                }

                value2 = DefaultValueConverter.Instance.Convert(value2, TypeAcceptedByListener, _converterParameter,
                    _currentCulture);
            }
            
            if (_converter is null)
            {
                var c = Tools.NeedsConversion(value, TypeAcceptedByListener);
                if (c is ValueConversionStatus.Acceptable or ValueConversionStatus.Special)
                    return false;
                ConvertWithDefault(ref value);
                return true;
            }

            try
            {
                value = _converter.Convert(value, TypeAcceptedByListener, _converterParameter, _currentCulture);
                var c = Tools.NeedsConversion(value, TypeAcceptedByListener);
                if (c is ValueConversionStatus.Acceptable or ValueConversionStatus.Special)
                    return true;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            ConvertWithDefault(ref value);
            return true;
        }

        

        public object ConvertBack(object value, Type sourceType)
        {
            try
            {
                if (_converter is not null)
                    value = _converter.ConvertBack(value, sourceType, _converterParameter, _currentCulture);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            var c = Tools.NeedsConversion(value, sourceType);
            if (c is ValueConversionStatus.Acceptable or ValueConversionStatus.Special)
                return value;
            value = DefaultValueConverter.Instance.ConvertBack(value, sourceType, _converterParameter,
                _currentCulture);
            c     = Tools.NeedsConversion(value, sourceType);
            if (c is ValueConversionStatus.Acceptable or ValueConversionStatus.Special)
                return value;
            return BindingSpecial.Invalid;
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
                void DispatcherMethod() { _action(info); }

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
        private readonly string _stringFormat;
        
        public Type   TypeAcceptedByListener { get; }
        public object Tag                    { get; }
    }
}
