using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    public interface IBindingValueWrapper
    {
        #region properties

        public object Source { get; }

        #endregion
    }

    internal sealed class BindingValueWrapper : DisposableBase, IBindingValueWrapper
    {
        public BindingValueWrapper(object source, BindingManager bindingManager)
            : base(DisposingStateBehaviour.None)
        {
            Source         = source;
            BindingManager = bindingManager;
        }

        [CanBeNull]
        internal IDisposable AddListenerAction(ListerInfo listener)
        {
            ThrowIfDisposed();
            if (_listeners.Contains(listener))
                return null;
            _listeners.Add(listener);

            var infoStart = Create1(ListenerDelegateKind.StartBinding);
            InvokeListener(listener, infoStart);

            return new DisposableAction(() =>
            {
                if (!_listeners.Contains(listener))
                    return;
                _listeners.Remove(listener);
                var infoEnd = Create1(ListenerDelegateKind.EndBinding);
                InvokeListener(listener, infoEnd);
                if (_listeners.Count == 0 && _properties.Count == 0)
                {
                    Dispose();
                }
            });
        }

        private PrvValueInfo Create1(ListenerDelegateKind kind)
        {
            if (kind == ListenerDelegateKind.EndBinding)
                return new PrvValueInfo(kind, BindingSpecial.Unbound, _lastValidSource);
            var infoStart = new PrvValueInfo(kind, _source, _lastValidSource);
            return infoStart;
        }

        public BindingValueWrapper CreateAccessor(string path)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(path))
                return this;
            var parts = path.Split('.');

            var current = this;
            for (var i = 0; i < parts.Length; i++)
            {
                var p    = parts[i];
                var next = current.GetOrCreate(p);
                current = next;
            }

            return current;
        }

        protected override void DisposeInternal(bool disposing)
        {
            PropertyChangedEventsListeningEnd();

            _source = null;
            if (_ownerPropertyName != null && _owner != null)
            {
                _owner.RemoveProperty(_ownerPropertyName);
                _ownerPropertyName = null;
                _owner             = null;
            }

            if (_properties.Count > 0)
            {
                var toDispose = _properties.Values.ToArray();
                for (var index = 0; index < toDispose.Length; index++)
                {
                    var i = toDispose[index];
                    i.Dispose();
                }

                _properties.Clear();
            }

            if (_listeners.Count > 0)
            {
                var info = Create1(ListenerDelegateKind.EndBinding);
                for (var index = 0; index < _listeners.Count; index++)
                {
                    var lister = _listeners[index];
                    InvokeListener(lister, info);
                }

                _listeners.Clear();
            }

            base.DisposeInternal(disposing);
        }

        private void ForwardSourceToListeners(bool force)
        {
            if (!force && !_forwardSourceToListeners) return;
            _forwardSourceToListeners = false;

            if (_accessor != null)
                if (!_accessor.TryChangeSource(_source))
                    _accessor = null;
            SureAccessor();
            if (_properties.Count > 0)
                foreach (var pair in _properties)
                {
                    var propertyValue = _accessor.Read(pair.Key);
                    pair.Value.Source = propertyValue;
                }

            if (_listeners.Count > 0)
            {
                var kind = ListenerDelegateKind.ValueChanged;
                if ((_flags & ValuePropagationFlags.UpdateSource) != 0)
                    kind = ListenerDelegateKind.UpdateSource;
                var info = Create1(kind);
                for (var index = 0; index < _listeners.Count; index++)
                {
                    var listener = _listeners[index];
                    InvokeListener(listener, info);
                }
            }
        }

        private BindingValueWrapper GetOrCreate(string propertyName)
        {
            if (!_properties.TryGetValue(propertyName, out var wrapper))
            {
                var value                            = GetPropertyValue(propertyName);
                _properties[propertyName]  = wrapper = new BindingValueWrapper(value, BindingManager);
                wrapper._owner             = this;
                wrapper._ownerPropertyName = propertyName;
                if (_properties.Count == 1)
                    PropertyChangedEventsListeningBegin();
            }

            return wrapper;
        }

        private object GetPropertyValue(string propertyName)
        {
            if (_source is BindingSpecial special)
            {
                switch (special.Kind)
                {
                    case BindingSpecialKind.NotSet:
                    case BindingSpecialKind.Unbound:
                        return special;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            if (_source is null)
                return BindingSpecial.NotSet;

            SureAccessor();
            return _accessor.Read(propertyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeListener(ListerInfo listener, IValueInfo infoEnd)
        {
            if (BindingTracker.Instance.LoggingEnabled)
                BindingTracker.Instance.LogInvokeingListener(this, infoEnd, listener);
            listener.Invoke(infoEnd);
        }

        private void PropertyChangedEventsListeningBegin()
        {
            if (_isListening) return;
            if (Source is INotifyPropertyChanged npc)
                npc.PropertyChanged += SourcePropertyChanged;
            _isListening = true;
        }

        private void PropertyChangedEventsListeningEnd()
        {
            if (!_isListening) return;
            if (Source is INotifyPropertyChanged npc)
                npc.PropertyChanged -= SourcePropertyChanged;
            _isListening = false;
        }

        private void RemoveProperty(string propertyName)
        {
            if (_properties.Count == 0)
                return;
            _properties.Remove(propertyName);
            if (_properties.Count == 0)
            {
                if (_listeners.Count == 0)
                    Dispose();
                else
                    PropertyChangedEventsListeningEnd();
            }
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var key = e.PropertyName;
            if (key is null)
                return;
            if (_properties.TryGetValue(key, out var value))
            {
                SureAccessor();
                var propertyValue = _accessor.Read(key);
                value.Source = propertyValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SureAccessor()
        {
            if (_accessor is not null)
                return;
            ObjectAccessorMaker.SureAccessor(_source, ref _accessor);
        }

        internal UpdateSourceResult UpdateSource(object value, ListerInfo listerInfo,
            IReadOnlyList<BindingValidator> bindingValidators)
        {
            try
            {
                ThrowIfDisposed();
                if ((_flags & ValuePropagationFlags.UpdateSource) != 0)
                    return UpdateSourceResult.NotSet;
                _flags |= ValuePropagationFlags.UpdateSource;
                _flags &= ~ValuePropagationFlags.SourceSetValueWasInvoked;
                try
                {
                    if (_owner is null)
                    {
                        Source = value;
                        return UpdateSourceResult.OkFromValue(value);
                    }

                    try
                    {
                        _flags |= ValuePropagationFlags.OwnerUpdateSource;
                        var ownerResult = _owner.UpdateSource(_ownerPropertyName, value, listerInfo, bindingValidators);
                        _flags &= ~ValuePropagationFlags.OwnerUpdateSource;
                        var wasUpdatedBack = (_flags & ValuePropagationFlags.SourceSetValueWasInvoked) != 0;
                        if (wasUpdatedBack)
                        {
                            _flags &= ~ValuePropagationFlags.SourceSetValueWasInvoked;
                        }

                        if (ownerResult.Exception != null)
                        {
                            Source = BindingSpecial.Invalid;
                            return ownerResult;
                        }

                        if (ownerResult.Status == UpdateSourceResultStatus.Ok)
                        {
                            if (!Equals(value, ownerResult.Value))
                            {
                                Source = ownerResult.Value;
                            }
                        }

                        return ownerResult;
                    }
                    finally
                    {
                        ForwardSourceToListeners(false);
                    }
                }
                finally
                {
                    _flags &= ~ValuePropagationFlags.UpdateSource;
                }
            }
            catch (Exception e)
            {
                return UpdateSourceResult.FromException(e, value);
            }
        }

        private UpdateSourceResult UpdateSource(string propertyName, object value, ListerInfo listerInfo,
            IReadOnlyList<BindingValidator> bindingValidators)
        {
            ThrowIfDisposed();
            SureAccessor();
            if (listerInfo != null)
            {
                var propertyType = _accessor.GetPropertyType(propertyName);
                value = listerInfo.ConvertBack(value, propertyType);
            }

            if (value is BindingSpecial s)
            {
                if (s == BindingSpecial.Invalid)
                    return UpdateSourceResult.InvalidValue;
                return UpdateSourceResult.Special;
            }

            if (value is UpdateSourceResult afterConversion)
                return afterConversion;

            try
            {
                if (bindingValidators != null && bindingValidators.Count > 0)
                {
                    for (var index = 0; index < bindingValidators.Count; index++)
                    {
                        var validator       = bindingValidators[index];
                        var validatorResult = validator.Check(value);
                        if (validatorResult.IsOk) continue;
                        var exception = new Exception(validatorResult.ErrorMessage);
                        return UpdateSourceResult.FromException(exception, value);
                    }
                }

                /*if (BindingTracker.Instance.LoggingEnabled)
                {
                    BindingTracker.Instance.LogSourceChanging(this, value);
                }*/

                var result = _accessor.Write(propertyName, value);
                return result;
            }
            catch (Exception e)
            {
                return UpdateSourceResult.FromException(e, value);
            }
        }

        #region properties

        public BindingManager BindingManager { get; }

        #endregion

        public object Source
        {
            get => _source;
            private set
            {
                ThrowIfDisposed();
                var isUnderOwnerUpdateSource = (_flags & ValuePropagationFlags.OwnerUpdateSource) != 0;
                if (isUnderOwnerUpdateSource)
                    _flags |= ValuePropagationFlags.SourceSetValueWasInvoked;
                if (ReferenceEquals(_source, value))
                    return;
                if (_isListening && _source is INotifyPropertyChanged oldNpc)
                    oldNpc.PropertyChanged -= SourcePropertyChanged;
                {
                    if (BindingTracker.Instance.LoggingEnabled)
                    {
                        BindingTracker.Instance.LogSourceChanging(this, value);
                    }
                }
                _source = value;

                if (value is not BindingSpecial)
                {
                    _lastValidSource = _source;
                    if (_isListening && _source is INotifyPropertyChanged newNpc)
                        newNpc.PropertyChanged += SourcePropertyChanged;
                }

                if (isUnderOwnerUpdateSource)
                    _forwardSourceToListeners = true;
                else
                    ForwardSourceToListeners(true);
            }
        }

        #region Fields

        private bool _forwardSourceToListeners = true;

        private readonly List<ListerInfo> _listeners = new();
        private readonly Dictionary<string, BindingValueWrapper> _properties = new();
        private IObjectAccessor _accessor;

        private ValuePropagationFlags _flags;
        private bool _isListening;
        private object _lastValidSource;
        private BindingValueWrapper _owner;
        private string _ownerPropertyName;
        private object _source;

        #endregion
    }


    [Flags]
    internal enum ValuePropagationFlags
    {
        None = 0,
        UpdateSource = 1,
        OwnerUpdateSource = 2,
        SourceSetValueWasInvoked = 4
    }
}
