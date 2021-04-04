using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Social.Extensions;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests.Extensions
{
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

        public HtmlWebExtensionsTests(ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(
            "../../../json/social-twitter/bryan-wilhite/social-data-test.json",
            "https://codeopinion.com/avoiding-nullreferenceexception/")]
        [InlineData(
            "../../../json/social-twitter/kinte-space/social-data-test.json",
            "https://www.wired.com/story/smoke-from-wildfires-is-a-growing-public-health-crisis-for-cities/")]
        public void ToSocialData_Test(string target, string location)
        {
            target = ProgramAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, target);
            Assert.True(File.Exists(target));

            JObject jO = null;

            using (var writer = new StringWriter())
            using (var listener = new TextWriterTraceListener(writer))
            {
                ProgramUtility.InitializeTraceSource(listener);

                jO = new HtmlWeb().WithChromeishUserAgent().ToSocialData(location);

                listener.Flush();
                this._testOutputHelper.WriteLine(writer.ToString());
            }

            Assert.NotNull(jO);

            File.WriteAllText(target, jO.ToString());
        }

        readonly ITestOutputHelper _testOutputHelper;
    }
}
