namespace iSukces.Binding.Test.Data
{
    class SimpleNpc : NpcBase
    {
        public SimpleNpc(StringBuilder log, string name)
            : base(log, name)
        {
        }

        public string Title { get => _title; set => SetAndNotify(ref _title, value); }

        public bool IsChecked { get => _isChecked; set => SetAndNotify(ref _isChecked, value); }

        private bool _isChecked;
        private string _title;
    }
}
