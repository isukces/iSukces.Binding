using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    internal sealed class BindingValueWrapper : DisposableBase
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
            listener.Invoke(infoStart);

            return new DisposableAction(() =>
            {
                if (!_listeners.Contains(listener))
                    return;
                _listeners.Remove(listener);
                var infoEnd = Create1(ListenerDelegateKind.EndBinding);
                listener.Invoke(infoEnd);
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
                foreach (var i in _properties.Values)
                    i.Dispose();
                _properties.Clear();
            }

            if (_listeners.Count > 0)
            {
                var info = Create1(ListenerDelegateKind.EndBinding);
                foreach (var i in _listeners)
                    i.Invoke(info);
                _listeners.Clear();
            }

            base.DisposeInternal(disposing);
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

        internal UpdateSourceResult UpdateSource(object value, ListerInfo listerInfo)
        {
            try
            {
                ThrowIfDisposed();
                if ((_flags & ValuePropagationFlags.UpdateSource) != 0)
                    return UpdateSourceResult.NotSet;
                _flags |= ValuePropagationFlags.UpdateSource;
                try
                {
                    if (_owner is null)
                    {
                        Source = value;
                        return UpdateSourceResult.Ok;
                    }

                    var ownerResult = _owner.UpdateSource(_ownerPropertyName, value, listerInfo);
                    if (ownerResult.Exception != null)
                    {
                        Source = BindingSpecial.Invalid;
                    }

                    return ownerResult;
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

        private UpdateSourceResult UpdateSource(string propertyName, object value, ListerInfo listerInfo)
        {
            ThrowIfDisposed();
            SureAccessor();
            if (listerInfo != null)
                value = listerInfo.ConvertBack(value, _accessor.GetPropertyType(propertyName));

            if (value is BindingSpecial )
            {
                return UpdateSourceResult.Special;
            }

            
            try
            {
                var result = _accessor.Write(propertyName, value);
                return result;
            }
            catch (Exception e)
            {
                return UpdateSourceResult.FromException(e, value);
            }
        }

        public object Source
        {
            get => _source;
            private set
            {
                ThrowIfDisposed();
                if (ReferenceEquals(_source, value))
                    return;
                if (_isListening && value is INotifyPropertyChanged oldNpc)
                    oldNpc.PropertyChanged -= SourcePropertyChanged;
                _source = value;
                if (value is not BindingSpecial)
                    _lastValidSource = _source;
                if (_isListening && value is INotifyPropertyChanged newNpc)
                    newNpc.PropertyChanged += SourcePropertyChanged;

                if (_accessor != null)
                    if (!_accessor.TryChangeSource(_source))
                        _accessor = null;
                SureAccessor();
                foreach (var pair in _properties)
                {
                    var propertyValue = _accessor.Read(pair.Key);
                    pair.Value.Source = propertyValue;
                }

                for (var index = 0; index < _listeners.Count; index++)
                {
                    var listener = _listeners[index];
                    var kind     = ListenerDelegateKind.ValueChanged;
                    if ((_flags & ValuePropagationFlags.UpdateSource) != 0)
                        kind = ListenerDelegateKind.UpdateSource;
                    var info = Create1(kind);
                    listener.Invoke(info);
                }
            }
        }

        public BindingManager BindingManager { get; }

        private readonly List<ListerInfo> _listeners = new();
        private readonly Dictionary<string, BindingValueWrapper> _properties = new();
        private IObjectAccessor _accessor;

        private ValuePropagationFlags _flags;
        private bool _isListening;
        private object _lastValidSource;
        private BindingValueWrapper _owner;
        private string _ownerPropertyName;
        private object _source;
    }


    [Flags]
    internal enum ValuePropagationFlags
    {
        None = 0,
        UpdateSource = 1
    }
}
