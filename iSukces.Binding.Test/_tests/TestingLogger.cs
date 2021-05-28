using System;
using System.Text;
using Xunit.Abstractions;

namespace iSukces.Binding.Test
{
    class TestingLogger : ITestingLogger
    {
        public TestingLogger(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _sb         = new StringBuilder();
            _starting   = DateTime.Now;
        }

        public void WriteLine(string text)
        {
            var stamp = DateTime.Now - _starting;
            _testOutput.WriteLine("{0:f3} ms: {1}", stamp.TotalMilliseconds, text);
            _sb.AppendLine(text);
        }

        public override string ToString() { return _sb.ToString().Trim(); }
        private readonly StringBuilder _sb;

        private readonly ITestOutputHelper _testOutput;
        private readonly DateTime _starting;
    }
}
