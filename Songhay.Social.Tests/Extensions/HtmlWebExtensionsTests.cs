using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Polly;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Social.Activities;
using Songhay.Social.Extensions;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Tests.Extensions;

public class HtmlWebExtensionsTests
{
    static HtmlWebExtensionsTests()
    {
        TraceSources.ConfiguredTraceSourceName = $"trace-{nameof(HtmlWebExtensionsTests)}";

        TraceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();
    }

    static readonly TraceSource? TraceSource;

    static readonly AsyncPolicy RetryPolicy = UniformResourceActivity.RetryPolicy; 

    public HtmlWebExtensionsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(
        "../../../json/social-twitter/bryan-wilhite/social-data-test.json",
        "https://codeopinion.com/avoiding-nullreferenceexception/")]
    [InlineData(
        "../../../json/social-twitter/kinte-space/social-data-test.json",
        "https://www.wired.com/story/smoke-from-wildfires-is-a-growing-public-health-crisis-for-cities/")]
    public async Task ToSocialData_Test(string target, string location)
    {
        target = ProgramAssemblyUtility.GetPathFromAssembly(GetType().Assembly, target);
        Assert.True(File.Exists(target));

        JObject? jO = null;

        await using (var writer = new StringWriter())
        using (var listener = new TextWriterTraceListener(writer))
        {
            ProgramUtility.InitializeTraceSource(listener);

            jO = await new HtmlWeb().WithChromeishUserAgent().ToSocialDataAsync(location, RetryPolicy);

            listener.Flush();
            _testOutputHelper.WriteLine(writer.ToString());
        }

        Assert.NotNull(jO);

        await File.WriteAllTextAsync(target, jO.ToString());
    }

    readonly ITestOutputHelper _testOutputHelper;
}