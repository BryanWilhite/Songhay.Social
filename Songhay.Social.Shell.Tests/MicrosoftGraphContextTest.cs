using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace Songhay.Social.Shell.Tests
{
    [TestClass]
    public class MicrosoftGraphContextTest
    {
        static MicrosoftGraphContextTest()
        {
            httpClient = new HttpClient();
        }

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            var targetDirectoryInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            var basePath = targetDirectoryInfo.FullName;
            var meta = new ProgramMetadata();
            var configuration = this.TestContext.ShouldLoadConfigurationFromConventionalProject(this.GetType(), b =>
            {
                b.AddJsonFile("./app-settings.songhay-system.json", optional: false, reloadOnChange: false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), meta);

            restApiMetadata = meta.RestApiMetadataSet.TryGetValueWithKey("MicrosoftGraph", throwException: true);
        }

        [TestMethod]
        [TestProperty("redirectLocation", "https://localhost:44334/signin-oidc")]
        [TestProperty("uriTemplateKey", "oauth2-authorization")]
        public async Task ShouldGetAuthorizationCode()
        {
            #region test properties:

            var redirectLocation = this.TestContext.Properties["redirectLocation"].ToString();
            var uriTemplateKey = this.TestContext.Properties["uriTemplateKey"].ToString();

            #endregion

            var template = string.Concat(
                restApiMetadata.ClaimsSet.TryGetValueWithKey("authority"),
                restApiMetadata.UriTemplates.TryGetValueWithKey(uriTemplateKey));
            this.TestContext.WriteLine($"URI template: {template}");

            var uriTemplate = new UriTemplate(template);
            var scope = restApiMetadata.ClaimsSet.TryGetValueWithKey("scopes").Replace(',', ' ');
            var uri = uriTemplate.BindByPosition(restApiMetadata.ApiKey, redirectLocation, scope);
            this.TestContext.WriteLine($"URI: {uri.OriginalString}");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(requestMessage);

            var content = await response.Content.ReadAsStringAsync();

            this.TestContext.WriteLine(content);
            Assert.IsTrue(response.IsSuccessStatusCode, "The expected success status code is not here.");
        }

        static readonly HttpClient httpClient;

        static RestApiMetadata restApiMetadata;
    }
}
