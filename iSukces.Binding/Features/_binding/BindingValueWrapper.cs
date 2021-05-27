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
        public IDisposable AddListenerAction(ListenerDelegate listener)
        {
            ThrowIfDisposed();
            if (_listeners.Contains(listener))
                return null;
            _listeners.Add(listener);

            listener.Invoke(_source, ListenerDelegateKind.StartBinding);

            return new DisposableAction(() =>
            {
                if (!_listeners.Contains(listener))
                    return;
                _listeners.Remove(listener);
                listener.Invoke(BindingSpecial.Unbound, ListenerDelegateKind.EndBinding);
                if (_listeners.Count == 0 && _properties.Count == 0)
                {
                    Dispose();
                }
            });
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
                foreach (var i in _listeners)
                    i.Invoke(BindingSpecial.Unbound, ListenerDelegateKind.EndBinding);
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
            if (_source is BindingSpecial s)
            {
                switch (s.Kind)
                {
                    case BindingSpecialKind.NotSet:
                    case BindingSpecialKind.Unbound:
                        return s;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            if (_source is null)
                return BindingSpecial.NotSet;

            SureAccessor();
            return _accessor[propertyName];
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
                var propertyValue = _accessor[key];
                value.Source = propertyValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SureAccessor()
        {
            if (_accessor is not null)
                return;
            PropertyAccessorMaker.SureAccessor(_source, ref _accessor);
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
                if (_isListening && value is INotifyPropertyChanged newNpc)
                    newNpc.PropertyChanged += SourcePropertyChanged;

                if (_accessor != null)
                    if (!_accessor.TryChangeSource(_source))
                        _accessor = null;
                SureAccessor();
                foreach (var pair in _properties)
                {
                    var propertyValue = _accessor[pair.Key];
                    pair.Value.Source = propertyValue;
                }

                for (var index = 0; index < _listeners.Count; index++)
                {
                    var listener = _listeners[index];
                    listener.Invoke(_source, ListenerDelegateKind.ValueChanged);
                }
            }
        }

        public BindingManager BindingManager { get; }

        private readonly List<ListenerDelegate> _listeners = new();
        private readonly Dictionary<string, BindingValueWrapper> _properties = new();
        private IPropertyAccessor _accessor;
        private bool _isListening;
        private BindingValueWrapper _owner;
        private string _ownerPropertyName;
        private object _source;
    }

    public delegate void ListenerDelegate(object value, ListenerDelegateKind kind);

    public enum ListenerDelegateKind
    {
        ValueChanged,
        StartBinding,
        EndBinding
    }
}
