using Newtonsoft.Json.Linq;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Social.Extensions;
using Songhay.Social.Models;
using System.Diagnostics;
using System.IO;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests.Extensions
{
    public class FileInfoExtensionsTests
    {
        static FileInfoExtensionsTests()
        {
            TraceSources.ConfiguredTraceSourceName = $"trace-{nameof(FileInfoExtensionsTests)}";

            traceSource = TraceSources
                .Instance
                .GetTraceSourceFromConfiguredName()
                .WithSourceLevels();
        }

        static TraceSource traceSource;

        public FileInfoExtensionsTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;
        }

        [Theory]
        [InlineData(
            "./TweetBooks/TweetBook-{year}-{month}.xlsx",
            "2021",
            "04",
            7,
            "../../../../../azure-storage-accounts/songhaystorage/social-twitter")]
        public void PartitionSocialData_Test(string pathExpression, string year, string month, int partitionSize, string targetRoot)
        {
            var projectRoot = ProgramAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var pathTemplate = new UriTemplate(pathExpression);

            var path = pathTemplate.BindByPosition(year, month)?.OriginalString;
            path = projectInfo.ToCombinedPath(path);
            Assert.True(File.Exists(path));

            targetRoot = ProgramAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, targetRoot);
            var targetRootInfo = new DirectoryInfo(targetRoot);
            Assert.True(targetRootInfo.Exists);

            var fileInfo = new FileInfo(path);

            void PartitionAction(SocialPartition partition)
            {
                var jsonPathName = partition.GroupName.EqualsInvariant("BryanWilhite") ? "bryan-wilhite" : "kinte-space";
                var jsonFileName = $"{Path.GetFileNameWithoutExtension(fileInfo.FullName)}-{partition.GroupName}-{partition.PartitionOrdinal:000}.json".ToLower();
                var jA = new JArray(partition.Data);
                File.WriteAllText(targetRootInfo.ToCombinedPath($"{jsonPathName}/{jsonFileName}"), jA.ToString());
            }

            using (var writer = new StringWriter())
            using (var listener = new TextWriterTraceListener(writer))
            {
                ProgramUtility.InitializeTraceSource(listener);

                fileInfo.PartitionSocialData(partitionSize, PartitionAction);

                listener.Flush();
                this._testOutputHelper.WriteLine(writer.ToString());
            }
        }

        readonly ITestOutputHelper _testOutputHelper;
    }
}
