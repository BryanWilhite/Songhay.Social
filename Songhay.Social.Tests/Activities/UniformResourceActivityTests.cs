using System.Text.Json;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Activities;
using Songhay.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Tests.Activities;

public class UniformResourceActivityTests
{
    public UniformResourceActivityTests(ITestOutputHelper helper)
    {
        _testOutputHelper = helper;
    }

    [Fact]
    public void DisplayHelp_Test()
    {
        var activity = new UniformResourceActivity();

        var actual = activity.DisplayHelp(new ProgramArgs(new[] { ProgramArgs.Help }));
        Assert.False(string.IsNullOrWhiteSpace(actual));
        _testOutputHelper.WriteLine(actual);
    }

    [Theory]
    [ProjectFileData(typeof(UniformResourceActivityTests),
        "../../../json/GenerateTwitterPartitions_Test_input.json",
        "../../../json/GenerateTwitterPartitions_Test_output.json")]
    public async Task GenerateTwitterPartitions_Test(FileInfo inputInfo, FileInfo outputInfo)
    {
        var uris = JsonSerializer
            .Deserialize<string[]>(await File.ReadAllTextAsync(inputInfo.FullName));

        var activity = new UniformResourceActivity();
        var output = await UniformResourceActivity.GenerateTwitterPartitionAsync(uris.ToReferenceTypeValueOrThrow());

        Assert.False(string.IsNullOrWhiteSpace(output));
        _testOutputHelper.WriteLine(output);

        await File.WriteAllTextAsync(outputInfo.FullName, output);
    }

    readonly ITestOutputHelper _testOutputHelper;
}