using System;
using System.Globalization;
using System.Text;

namespace iSukces.Binding.Test.Data
{
    class SimpleNpc : NpcBase
    {
        public SimpleNpc(ITestingLogger log, string name)
            : base(log, name)
        {
        }

        public string Title { get => _title; set => SetAndNotify(ref _title, value); }

        public bool IsChecked { get => _isChecked; set => SetAndNotify(ref _isChecked, value); }

        public int IntNumber { get => _intNumber; set => SetAndNotify(ref _intNumber, value); }

        public decimal DecimalNumber
        {
            get => _decimalNumber;
            set
            {
                if (value < 0)
                    throw new ArgumentException("value cannot be negative");
                if (value > 1000)
                    value = 1000;
                // silent modification
                value = Math.Round(value, 2);
                //_decimalNumber = Math.Round(_decimalNumber, 2);
                SetAndNotify(ref _decimalNumber, value);
            }
        }

        private decimal _decimalNumber;


        private int _intNumber;
        private bool _isChecked;
        private string _title;
    }
}
