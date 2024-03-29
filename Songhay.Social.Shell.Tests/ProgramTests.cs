using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Activities;
using Songhay.Tests;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests;

public class ProgramTests
{
    public ProgramTests(ITestOutputHelper helper)
    {
        _testOutputHelper = helper;
    }

    [Theory]
    [InlineData(
        "./TweetBooks/TweetBook-{year}-{month}.xlsx", 2018, 5, 7,
        "./json/PartitionRows_Test_output")]
    public void IExcelDataReaderActivity_Test(string pathExpression, int year, int month, int partitionSize, string partitionRoot)
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

        var args = new []
        {
            nameof(IExcelDataReaderActivity),
            IExcelDataReaderActivity.ArgExcelFile, excelPath,
            IExcelDataReaderActivity.ArgPartitionRoot, partitionRoot,
            IExcelDataReaderActivity.ArgPartitionSize, $"{partitionSize}",
        };

        Program.Run(args);
    }


    [Theory]
    [ProjectFileData(typeof(ProgramTests),
        "../../../json/UniformResourceActivity_Test_input.json",
        "../../../json/UniformResourceActivity_Test_output.json")]
    public void UniformResourceActivity_Test(FileInfo inputInfo, FileInfo outputInfo)
    {
        var args = new []
        {
            nameof(UniformResourceActivity),
            ProgramArgs.InputFile, inputInfo.FullName,
            ProgramArgs.OutputFile, outputInfo.FullName,
        };

        Program.Run(args);
    }

    readonly ITestOutputHelper _testOutputHelper;
}