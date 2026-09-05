using EFFluentify.Infrastructure.IO;
using Xunit;

namespace EFFluentify.Tests.Unit.IO
{
    // Shares the sequential collection because it redirects the process-wide Console streams.
    [Trait("Category", "Unit")]
    [Collection("Sequential Tests")]
    public class ConsoleManagerTests
    {
        [Fact]
        public void WriteLine_WritesMessageToStdout()
        {
            var original = Console.Out;
            using var sw = new StringWriter();
            Console.SetOut(sw);
            try { new ConsoleManager().WriteLine("hello world"); }
            finally { Console.SetOut(original); }

            Assert.Contains("hello world", sw.ToString());
        }

        [Fact]
        public void WriteError_WritesMessageToStderr()
        {
            var original = Console.Error;
            using var sw = new StringWriter();
            Console.SetError(sw);
            try { new ConsoleManager().WriteError("boom"); }
            finally { Console.SetError(original); }

            Assert.Contains("boom", sw.ToString());
        }

        [Fact]
        public void ReadLine_ReadsFromStdin()
        {
            var original = Console.In;
            Console.SetIn(new StringReader("typed input\n"));
            try { Assert.Equal("typed input", new ConsoleManager().ReadLine()); }
            finally { Console.SetIn(original); }
        }
    }
}
