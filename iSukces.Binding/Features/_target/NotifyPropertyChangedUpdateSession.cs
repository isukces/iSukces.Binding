using System;
using System.ComponentModel;

namespace iSukces.Binding
{
    public sealed class NotifyPropertyChangedUpdateSession : DisposableBase, IPropertyUpdateSession
    {
        public NotifyPropertyChangedUpdateSession(Action onPropertyChangedAction, object target, string propertyName)
            : base(DisposingStateBehaviour.None)
        {
            _onPropertyChangedAction = onPropertyChangedAction;
            _target                  = target;
            _propertyName            = propertyName;
            if (_target is INotifyPropertyChanged npc)
                npc.PropertyChanged += NpcOnPropertyChanged;
        }

        protected override void DisposeInternal(bool disposing)
        {
            if (_target is INotifyPropertyChanged npc)
                npc.PropertyChanged -= NpcOnPropertyChanged;
            base.DisposeInternal(disposing);
        }

        private void NpcOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _propertyName) 
                _onPropertyChangedAction();
        }

        private readonly Action _onPropertyChangedAction;
        private readonly string _propertyName;
        private readonly object _target;

        public void ForceUpdate()
        {
            _onPropertyChangedAction();
        }
    }
}
