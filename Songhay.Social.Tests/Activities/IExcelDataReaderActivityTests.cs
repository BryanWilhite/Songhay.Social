using System.Diagnostics;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Activities;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Tests.Activities;

// ReSharper disable once InconsistentNaming
public class IExcelDataReaderActivityTests
{
    static IExcelDataReaderActivityTests()
    {
        TraceSources.ConfiguredTraceSourceName = $"trace-{nameof(IExcelDataReaderActivityTests)}";

        TraceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();
    }

    static readonly TraceSource? TraceSource;


    public IExcelDataReaderActivityTests(ITestOutputHelper helper)
    {
        _testOutputHelper = helper;
    }

    [Fact]
    public void DisplayHelp_Test()
    {
        var activity = new IExcelDataReaderActivity();

        var actual = activity.DisplayHelp(new ProgramArgs(new[] { ProgramArgs.Help }));
        Assert.False(string.IsNullOrWhiteSpace(actual));
        _testOutputHelper.WriteLine(actual);
    }

    [Theory]
    [InlineData(
        "./TweetBooks/TweetBook-{year}-{month}.xlsx", 2018, 5, 7,
        "./json/PartitionRows_Test_output")]
    public void PartitionRows_Test(string pathExpression, int year, int month, int partitionSize, string partitionRoot)
    {
        var projectRoot = ProgramAssemblyUtility.GetPathFromAssembly(GetType().Assembly, "../../../");
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

        using var writer = new StringWriter();
        using var listener = new TextWriterTraceListener(writer);
        ProgramUtility.InitializeTraceSource(listener);

        var activity = new IExcelDataReaderActivity();
        activity.PartitionRows(excelPath, partitionSize, partitionRoot);

        _testOutputHelper.WriteLine(writer.ToString());
    }

    readonly ITestOutputHelper _testOutputHelper;
}