using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Web.Tests.Controllers
{
    public class TwitterControllerTests
    {
        public TwitterControllerTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;

            var projectRoot = FrameworkAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var basePath = projectInfo.Parent.FindDirectory("Songhay.Social.Web").FullName;

            var builder = Program.CreateWebHostBuilder(args: null, builderAction: (builderContext, configBuilder) =>
            {
                Assert.NotNull(builderContext);

                this._testOutputHelper.WriteLine($"configuring {nameof(TestServer)} with {nameof(basePath)}: {basePath}...");


                var env = builderContext.HostingEnvironment;
                Assert.NotNull(env);

                env.ContentRootPath = basePath;
                env.EnvironmentName = "Development";
                env.WebRootPath = $"{basePath}{Path.DirectorySeparatorChar}wwwroot";

                configBuilder.SetBasePath(env.ContentRootPath);
            });

            Assert.NotNull(builder);

            this._server = new TestServer(builder);

            this._testOutputHelper.WriteLine("initializing domain metadata...");
            this._meta = new ProgramMetadata();
            var configuration = ProgramUtility.LoadConfiguration(basePath, b =>
            {
                b.AddJsonFile(AppScalars.ConventionalSettingsFile, optional: false, reloadOnChange: false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), this._meta);
            this._testOutputHelper.WriteLine($"{this._meta}");
        }

        [Theory]
        [InlineData(@"{}", @"json\ShouldGetTwitterFavorites.json", "statuses")]
        public async Task ShouldGetTwitterFavorites(string headers, string outputFile, string path)
        {
            var projectRoot = FrameworkAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var headersSet = JsonConvert.DeserializeObject<Dictionary<string, string>>(headers);

            outputFile = projectInfo.ToCombinedPath(outputFile);
            Assert.True(File.Exists(outputFile));

            var pathTemplate = new UriTemplate(string.Concat(baseRoute, path));

            var pathUri = pathTemplate.BindByPosition();
            var client = this._server.CreateClient();
            var response = await client.GetAsync(pathUri);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content), "The expected content is not here.");
            var jA = JArray.Parse(content);
            File.WriteAllText(outputFile, jA.ToString());
        }

        const string baseRoute = "twitter/v1/";

        readonly ITestOutputHelper _testOutputHelper;
        readonly ProgramMetadata _meta;
        readonly TestServer _server;
    }
}
