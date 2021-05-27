using System.Text;

namespace iSukces.Binding.Test.Data
{
    class Wrap1Npc : NpcBase
    {
        public Wrap1Npc(StringBuilder log, string name)
            : base(log, name)
        {
        }

        public SimpleNpc TitleAndChecked { get => _titleAndChecked; set => SetAndNotify(ref _titleAndChecked, value); }

        private SimpleNpc _titleAndChecked;
    }
}
