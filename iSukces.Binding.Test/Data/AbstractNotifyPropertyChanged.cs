namespace iSukces.Binding.Test.Data
{
    abstract class AbstractNotifyPropertyChanged : INotifyPropertyChanged
    {
        protected AbstractNotifyPropertyChanged(string name) { Name = name; }

        protected abstract void Log(string txt);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetAndNotify<T>(ref T backField, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (Equals(backField, value))
                return false;
            backField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private event PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                propertyChanged += value;
                Log($"[{Name}] Subscribe PropertyChanged");
            }
            remove
            {
                Log($"[{Name}] Unubscribe PropertyChanged");
                propertyChanged -= value;
            }
        }

        public bool HasPropertyChangedSubscribers => propertyChanged is not null;

        public string Name { get; }
    }
}
