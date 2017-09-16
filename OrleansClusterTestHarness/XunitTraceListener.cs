using System.Diagnostics;
using Xunit.Abstractions;

namespace OrleansClusterTestHarness
{

    public class XunitTraceListener : TraceListener
    {
        private readonly ITestOutputHelper output;

        public XunitTraceListener(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override void WriteLine(string str)
        {
            output.WriteLine(str);
        }

        public override void Write(string str)
        {
            output.WriteLine(str);
        }
    }
}
