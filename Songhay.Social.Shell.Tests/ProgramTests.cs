using System.IO;
using Songhay.Extensions;
using Songhay.Social.Activities;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests
{
    public class ProgramTests
    {
        public ProgramTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;
        }

        [Theory]
        [InlineData(
            "./TweetBooks/TweetBook-{year}-{month}.xlsx", 2018, 5, 7,
            "./json/PartitionRows_Test_output")]
        public void IExcelDataReaderActivity_Test(string pathExpression, int year, int month, int partitionSize, string partitionRoot)
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

            var args = new []
            {
                nameof(IExcelDataReaderActivity),
                IExcelDataReaderActivity.argExcelFile, excelPath,
                IExcelDataReaderActivity.argPartitionRoot, partitionRoot,
                IExcelDataReaderActivity.argPartitionSize, $"{partitionSize}",
            };

            Program.Run(args);
        }

        readonly ITestOutputHelper _testOutputHelper;
    }
}