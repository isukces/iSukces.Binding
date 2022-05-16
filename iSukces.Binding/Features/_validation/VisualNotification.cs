using System;

namespace iSukces.Binding
{
    public sealed class VisualNotification
    {
        private VisualNotification() { }

        public void Notify(object target, UpdateSourceResult result)
        {
            var args = new NotifyVisualEventArgs(target, result);
            NotifyVisual?.Invoke(this, args);
        }

        public event EventHandler<NotifyVisualEventArgs> NotifyVisual;

        public static VisualNotification Instance => VisualNotificationHolder.SingleIstance;

        private static class VisualNotificationHolder
        {
            public static readonly VisualNotification SingleIstance = new();
        }

        public sealed class NotifyVisualEventArgs : EventArgs
        {
            public NotifyVisualEventArgs(object target, UpdateSourceResult result)
            {
                Target = target;
                Result = result;
            }

            public object             Target { get; }
            public UpdateSourceResult Result { get; }
        }
    }
}
