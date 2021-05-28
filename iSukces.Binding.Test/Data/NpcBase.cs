using System.Text;

namespace iSukces.Binding.Test.Data
{
    class NpcBase : AbstractNotifyPropertyChanged
    {
        public NpcBase(ITestingLogger log, string name)
            : base(name)
        {
            _log = log;
        }

        protected override void Log(string txt) { _log.WriteLine(txt); }

        private readonly ITestingLogger _log;
    }
}
