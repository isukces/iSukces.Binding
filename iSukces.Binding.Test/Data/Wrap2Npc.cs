namespace iSukces.Binding.Test.Data
{
    class Wrap2Npc : NpcBase
    {
        public Wrap2Npc(StringBuilder log, string name)
            : base(log, name)
        {
        }

        public Wrap1Npc NestedObject { get => _nestedObject; set => SetAndNotify(ref _nestedObject, value); }

        private Wrap1Npc _nestedObject;
    }
}
