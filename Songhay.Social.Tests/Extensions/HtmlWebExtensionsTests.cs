﻿using HtmlAgilityPack;
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

        traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();
    }

    static TraceSource traceSource;

    static readonly AsyncPolicy retryPolicy = UniformResourceActivity.retryPolicy; 

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

        JObject jO = null;

        using (var writer = new StringWriter())
        using (var listener = new TextWriterTraceListener(writer))
        {
            ProgramUtility.InitializeTraceSource(listener);

            jO = await new HtmlWeb().WithChromeishUserAgent().ToSocialDataAsync(location, retryPolicy);

            listener.Flush();
            _testOutputHelper.WriteLine(writer.ToString());
        }

        Assert.NotNull(jO);

        File.WriteAllText(target, jO.ToString());
    }

    readonly ITestOutputHelper _testOutputHelper;
}