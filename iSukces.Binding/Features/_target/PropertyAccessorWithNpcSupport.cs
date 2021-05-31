using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    internal sealed class PropertyAccessorWithNpcSupport : DisposableBase, IPropertyAccessor2
    {
        public PropertyAccessorWithNpcSupport(
            [CanBeNull] object target,
            [CanBeNull] PropertyInfo propertyInfo)
            : base(DisposingStateBehaviour.None)
        {
            _target       = target;
            _propertyInfo = propertyInfo;
            if (target is INotifyPropertyChanged npc)
                npc.PropertyChanged += NpcOnPropertyChanged;
        }

        private static void NpcOnPropertyChanged(object sender, PropertyChangedEventArgs e) { }

        protected override void DisposeInternal(bool disposing)
        {
            if (_target is INotifyPropertyChanged npc)
                npc.PropertyChanged -= NpcOnPropertyChanged;
            base.DisposeInternal(disposing);
        }

        public IDisposable SubscribePropertyNotification(UpdateSourceDe updateBackAction)
        {
            return new NotifyPropertyChangedUpdateSession(() =>
            {
                var value = _propertyInfo.GetValue(_target);
                updateBackAction(value);
            }, _target, _propertyInfo.Name);
        }


        public UpdateSourceResult PropertySetValue(object value)
        {
            if (_propertyInfo is null || _target is null)
                return UpdateSourceResult.NotSet;
            try
            {
                ThrowIfDisposed();
                return Tools.PropertySetValue(_propertyInfo, _target, value);
            }
            catch (Exception e)
            {
                return UpdateSourceResult.FromException(e, value);
            }
        }

        public Type PropertyType => _propertyInfo?.PropertyType;
        private readonly PropertyInfo _propertyInfo;
        private readonly object _target;
    }
}
