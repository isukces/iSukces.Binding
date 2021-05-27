namespace iSukces.Binding.Test.Data
{
    class NpcBase : AbstractNotifyPropertyChanged
    {
        public NpcBase(StringBuilder log, string name)
            : base(name)
        {
            _log = log;
        }

        protected override void Log(string txt) { _log.AppendLine(txt); }

        private readonly StringBuilder _log;
    }
}
