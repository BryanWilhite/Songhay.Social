using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using System.IO;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Songhay.Social.Tests.Controllers
{
    [TestClass]
    public class SocialControllerTest
    {
        [TestInitialize]
        public void InitializeTest()
        {

            var targetDirectoryInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            var basePath = targetDirectoryInfo.FullName;

            var builder = Program.GetWebHostBuilder(args: null, builderAction: (builderContext, configBuilder) =>
            {
                Assert.IsNotNull(builderContext, "The expected Web Host builder context is not here.");

                this.TestContext.WriteLine($"configuring {nameof(TestServer)} with {nameof(basePath)}: {basePath}...");


                var env = builderContext.HostingEnvironment;
                Assert.IsNotNull(env, "The expected Hosting Environment is not here.");

                env.ContentRootPath = basePath;
                env.EnvironmentName = "Development";
                env.WebRootPath = $"{basePath}{Path.DirectorySeparatorChar}wwwroot";

                configBuilder.SetBasePath(env.ContentRootPath);
            });

            Assert.IsNotNull(builder, "The expected Web Host builder is not here.");

            this._server = new TestServer(builder);

            this.TestContext.WriteLine("initializing domain metadata...");
            this._meta = new ProgramMetadata();
            var configuration = this.TestContext.ShouldLoadConfigurationFromConventionalProject(this.GetType(), b =>
            {
                b.AddJsonFile("./app-settings.songhay-system.json", optional: false, reloadOnChange: false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), this._meta);
            this.TestContext.WriteLine($"{this._meta}");
        }

        public TestContext TestContext { get; set; }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("pathTemplate", "twitter-statuses")]
        public async Task ShouldGetTwitterFavorites()
        {
            #region test properties:

            var pathTemplate = new UriTemplate(string.Concat(baseRoute, this.TestContext.Properties["pathTemplate"].ToString()));

            #endregion

            var path = pathTemplate.BindByPosition();
            var client = this._server.CreateClient();
            var response = await client.GetAsync(path);

            response.EnsureSuccessStatusCode();
        }

        const string baseRoute = "api/social/v1/";

        ProgramMetadata _meta;
        TestServer _server;
    }
}
