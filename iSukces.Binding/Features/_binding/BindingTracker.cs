using System;

namespace iSukces.Binding
{
    public sealed class BindingTracker
    {
        private BindingTracker() { }


        public void LogInvokeingListener(IBindingValueWrapper bindingValueWrapper, IValueInfo valueInfo,
            IListerInfo listener)
        {
            if (!LoggingEnabled) return;
            var h = InvokingListener;
            if (h is null) return;
            var args = new InvokingListenerEventArgs(bindingValueWrapper, valueInfo, listener);
            h.Invoke(this, args);
        }

        public void LogSourceChanging(IBindingValueWrapper bindingValueWrapper, object value)
        {
            if (!LoggingEnabled) return;
            var h = SourceChanging;
            if (h is null) return;
            var args = new SourceChangingEventArgs(bindingValueWrapper, value);
            h.Invoke(this, args);
        }

        public void LogSourceUpdated(UpdateSourceResult result, IListerInfo listener)
        {
            if (!LoggingEnabled) return;
            var h = SourceUpdated;
            if (h is null) return;
            var args = new SourceUpdatedEventArgs(result, listener);
            h.Invoke(this, args);
        }

        internal void NotifyDataModelChanged() { DataModelChanged?.Invoke(this, EventArgs.Empty); }

        public event EventHandler DataModelChanged;

        public event EventHandler<InvokingListenerEventArgs> InvokingListener;
        
        public event EventHandler<SourceChangingEventArgs> SourceChanging;

        public event EventHandler<SourceUpdatedEventArgs> SourceUpdated;

        public static BindingTracker Instance => BindingTrackerHolder.SingleIstance;

        public bool LoggingEnabled { get; set; }

        public static class BindingTrackerHolder
        {
            public static readonly BindingTracker SingleIstance = new BindingTracker();
        }

        public class BindingValueWrapperEventArgs : EventArgs
        {
            protected BindingValueWrapperEventArgs(IBindingValueWrapper bindingValueWrapper)
            {
                BindingValueWrapper = bindingValueWrapper;
            }

            public IBindingValueWrapper BindingValueWrapper { get; }
        }

        public sealed class InvokingListenerEventArgs : BindingValueWrapperEventArgs
        {
            public InvokingListenerEventArgs(IBindingValueWrapper bindingValueWrapper, IValueInfo valueInfo,
                IListerInfo listener)
                : base(bindingValueWrapper)
            {
                ValueInfo = valueInfo;
                Listener  = listener;
            }

            public IValueInfo  ValueInfo { get; }
            public IListerInfo Listener  { get; }
        }

        public sealed class SourceChangingEventArgs : BindingValueWrapperEventArgs
        {
            public SourceChangingEventArgs(IBindingValueWrapper bindingValueWrapper, object value)
                : base(bindingValueWrapper)
            {
                Value = value;
            }

            public object Value { get; }
        }

        public sealed class SourceUpdatedEventArgs : EventArgs
        {
            public SourceUpdatedEventArgs(UpdateSourceResult result, IListerInfo listener)
            {
                Result   = result;
                Listener = listener;
            }

            public UpdateSourceResult Result   { get; }
            public IListerInfo        Listener { get; }
        }
    }
}
