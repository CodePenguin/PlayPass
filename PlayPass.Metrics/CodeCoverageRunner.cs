using System;
using System.Diagnostics;
using System.IO;

namespace PlayPass.Metrics
{
    internal class CodeCoverageRunner
    {
        private readonly string _basePath;
        private readonly string _packagesPath;
        private readonly string _openCoverResultsFileName;
        private readonly string _reportOutputPath;
        private const int execution_timeout = 2 * 60 * 1000;

        public CodeCoverageRunner(string basePath)
        {
            _basePath = basePath;
            _packagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget\\packages");
            _openCoverResultsFileName = Path.Combine(_basePath, "results.xml");
            _reportOutputPath = Path.Combine(_basePath, "coverage");

        }

        private void ExecuteTestProject(string projectAssemblyName)
        {
            var openCoverExecutable = Path.Combine(_packagesPath, "opencover\\4.7.922\\tools\\OpenCover.Console.exe");
            var nunitConsoleExecutable = Path.Combine(_packagesPath, "NUnit.ConsoleRunner\\3.11.1\\tools\\nunit3-console.exe");
            var filter = "+[Play*]*";

            var arguments = string.Format(
                    " -register:user" +
                    " -target:\"{0}\"" +
                    " -targetargs:\"--noheader {1}.dll\"" +
                    " -filter:\"{2}\"" +
                    " -mergebyhash" +
                    " -mergeoutput" +
                    " -output:\"{3}\"" +
                    " -skipautoprops" +
                    " -returntargetcode",
                    nunitConsoleExecutable, projectAssemblyName, filter, _openCoverResultsFileName);

            var p = new Process();
            p.StartInfo.FileName = openCoverExecutable;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.UseShellExecute = false;
            p.OutputDataReceived += (a, b) => Console.WriteLine(b.Data);
            p.ErrorDataReceived += (a, b) => Console.WriteLine(b.Data);
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit(execution_timeout);

            if (p.ExitCode != 0) throw new Exception($"Test project {projectAssemblyName} exited with {p.ExitCode}");
        }

        private void GenerateReport()
        {
            var reportGeneratorExecutable = Path.Combine(_packagesPath, "ReportGenerator\\4.5.8\\tools\\net47\\reportgenerator.exe");
            var arguments = string.Format(
                " -reports:\"{0}\"" +
                " -targetdir:\"{1}\"",
                _openCoverResultsFileName, _reportOutputPath);

            var p = new Process();
            p.StartInfo.FileName = reportGeneratorExecutable;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.UseShellExecute = false;
            p.OutputDataReceived += (a, b) => Console.WriteLine(b.Data);
            p.ErrorDataReceived += (a, b) => Console.WriteLine(b.Data);
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit(execution_timeout);
        }

        public void Execute()
        {
            foreach (var fileName in Directory.EnumerateFiles(_basePath, "*.Test.dll")) {
                var projectAssemblyName = Path.GetFileNameWithoutExtension(fileName);
                ExecuteTestProject(projectAssemblyName);
            }
            GenerateReport();
        }
    }
}
