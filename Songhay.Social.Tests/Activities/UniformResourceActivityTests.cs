using System.IO;
using System.Text.Json;
using Songhay.Models;
using Songhay.Social.Activities;
using Songhay.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Tests.Activities
{
    public class UniformResourceActivityTests
    {
        public UniformResourceActivityTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;
        }

        [Fact]
        public void DisplayHelp_Test()
        {
            var activity = new UniformResourceActivity();

            var actual = activity.DisplayHelp(new ProgramArgs(new[] { ProgramArgs.Help }));
            Assert.False(string.IsNullOrWhiteSpace(actual));
            this._testOutputHelper.WriteLine(actual);
        }

        [Theory]
        [ProjectFileData(typeof(UniformResourceActivityTests),
            "../../../json/GenerateTwitterPartitions_Test_input.json",
            "../../../json/GenerateTwitterPartitions_Test_output.json")]
        public void GenerateTwitterPartitions_Test(FileInfo inputInfo, FileInfo outputInfo)
        {
            var uris = JsonSerializer
                .Deserialize<string[]>(File.ReadAllText(inputInfo.FullName));

            var activity = new UniformResourceActivity();
            var output = activity.GenerateTwitterPartitions(uris);

            Assert.False(string.IsNullOrWhiteSpace(output));
            this._testOutputHelper.WriteLine(output);

            File.WriteAllText(outputInfo.FullName, output);
        }

        readonly ITestOutputHelper _testOutputHelper;
    }
}