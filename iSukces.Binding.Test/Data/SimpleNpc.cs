using System.Text;

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

        public int IntNumber { get => _intNumber; set => SetAndNotify(ref _intNumber, value); }
        private int _intNumber;


        private bool _isChecked;
        private string _title;
    }
}
