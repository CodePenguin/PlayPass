using System.IO;
using System.Reflection;

namespace PlayPass.Metrics
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var executableDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var codeCoverageRunner = new CodeCoverageRunner(executableDirectory);
            codeCoverageRunner.Execute();
        }
    }
}