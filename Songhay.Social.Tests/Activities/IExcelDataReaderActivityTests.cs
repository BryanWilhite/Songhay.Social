using System.Diagnostics;
using System.IO;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Activities;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Tests.Activities
{
    public class IExcelDataReaderActivityTests
    {
        static IExcelDataReaderActivityTests()
        {
            TraceSources.ConfiguredTraceSourceName = $"trace-{nameof(IExcelDataReaderActivityTests)}";

            traceSource = TraceSources
                .Instance
                .GetTraceSourceFromConfiguredName()
                .WithSourceLevels();
        }

        static TraceSource traceSource;


        public IExcelDataReaderActivityTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;
        }

        [Fact]
        public void DisplayHelp_Test()
        {
            var activity = new IExcelDataReaderActivity();

            var actual = activity.DisplayHelp(new ProgramArgs(new[] { ProgramArgs.Help }));
            Assert.False(string.IsNullOrWhiteSpace(actual));
            this._testOutputHelper.WriteLine(actual);
        }

        [Theory]
        [InlineData(
            "./TweetBooks/TweetBook-{year}-{month}.xlsx", 2018, 5, 7,
            "./json/PartitionRows_Test_output")]
        public void PartitionRows_Test(string pathExpression, int year, int month, int partitionSize, string partitionRoot)
        {
            var projectRoot = ProgramAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var pathTemplate = new UriTemplate(pathExpression);

            var excelPath = pathTemplate
                .BindByPosition($"{year}", $"{month:00}")?
                .OriginalString;

            excelPath = projectInfo.ToCombinedPath(excelPath);
            Assert.True(File.Exists(excelPath));

            partitionRoot = projectInfo.ToCombinedPath(partitionRoot);
            Assert.True(Directory.Exists(partitionRoot));

            var partitionRootInfo = new DirectoryInfo(partitionRoot);
            foreach (var directory in partitionRootInfo.GetDirectories())
            {
                directory.Delete(recursive: true);
            }

            using (var writer = new StringWriter())
            using (var listener = new TextWriterTraceListener(writer))
            {
                ProgramUtility.InitializeTraceSource(listener);

                var activity = new IExcelDataReaderActivity();
                activity.PartitionRows(excelPath, partitionSize, partitionRoot);

                this._testOutputHelper.WriteLine(writer.ToString());
            }
        }

        readonly ITestOutputHelper _testOutputHelper;
    }
}