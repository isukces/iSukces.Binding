using System.Text;
using iSukces.Binding.Test.Data;
using Xunit;

namespace iSukces.Binding.Test
{
    public class BindingManagerTests
    {
        [Theory]
        [InlineData("dispose listener")]
        [InlineData("dispose binding manager")]
        public void T01_Should_bind_change_and_dispose(string testNumber)
        {
            var bm  = new BindingManager();
            var log = new StringBuilder();

            var obj = new SimpleNpc(log, "Obj");
            obj.Title = "initial value";
            Assert.False(obj.HasPropertyChangedSubscribers);

            var binding = bm.From(obj);
            Assert.False(obj.HasPropertyChangedSubscribers);
            binding.WithPath(nameof(obj.Title));
            Assert.False(obj.HasPropertyChangedSubscribers);
            int changessCount = 0;
            var listener = binding.CreateListener((q, kind) =>
            {
                log.AppendLine($"Got value {kind}: '{q}'");
                changessCount++;
            });
            Assert.Equal(1, changessCount);
            Assert.True(obj.HasPropertyChangedSubscribers);
            obj.Title = "new Value";
            Assert.Equal(2, changessCount);

            if (testNumber == "dispose listener")
                listener.Dispose();
            if (testNumber == "dispose binding manager")
                bm.Dispose();

            Assert.False(obj.HasPropertyChangedSubscribers);

            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: 'initial value'
Got value ValueChanged: 'new Value'
Got value EndBinding: 'Unbound'
[Obj] Unubscribe PropertyChanged
";
            var log1 = log.ToString().Trim();
            Assert.Equal(expected.Trim(), log1);
        }
    }
}
