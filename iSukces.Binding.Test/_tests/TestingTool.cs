using Xunit.Abstractions;

namespace iSukces.Binding.Test
{
    internal sealed class TestingTool
    {
        public TestingTool(ITestOutputHelper testOutput) { log = new TestingLogger(testOutput); }

        public string GetLog() { return log.ToString().Trim(); }

        public void Listen(IValueInfo info)
        {
            if (info.Value is BindingSpecial)
                log.WriteLine($"Got value {info.Kind}: '{info.Value}', last valid '{info.LastValidValue}'");
            else
                log.WriteLine($"Got value {info.Kind}: '{info.Value}'");
            changessCount++;
        }

        public void WriteLine(string text) { log.WriteLine(text); }

        public readonly BindingManager bm = new BindingManager();
        public readonly ITestingLogger log;
        public int changessCount;
    }
}
