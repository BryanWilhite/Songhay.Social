using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Songhay.Extensions;
using Songhay.Models;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests
{
    public class MicrosoftGraphContextTest
    {
        public MicrosoftGraphContextTest(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;

            var projectRoot = FrameworkAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var basePath = projectInfo.Parent.FindDirectory("Songhay.Social.Web").FullName;
            var meta = new ProgramMetadata();
            var configuration = ProgramUtility.LoadConfiguration(basePath, b =>
            {
                b.AddJsonFile("./app-settings.songhay-system.json", optional : false, reloadOnChange : false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), meta);

            _restApiMetadata = meta.RestApiMetadataSet.TryGetValueWithKey("MicrosoftGraph", throwException : true);
        }

        [Theory]
        [InlineData("https://localhost:44334/signin-oidc", "oauth2-authorization")]
        public async Task ShouldGetAuthorizationCode(string redirectLocation, string uriTemplateKey)
        {
            var template = string.Concat(
                _restApiMetadata.ClaimsSet.TryGetValueWithKey("authority"),
                _restApiMetadata.UriTemplates.TryGetValueWithKey(uriTemplateKey));
            this._testOutputHelper.WriteLine($"URI template: {template}");

            var uriTemplate = new UriTemplate(template);
            var scope = _restApiMetadata.ClaimsSet.TryGetValueWithKey("scopes").Replace(',', ' ');
            var uri = uriTemplate.BindByPosition(_restApiMetadata.ApiKey, redirectLocation, scope);
            this._testOutputHelper.WriteLine($"URI: {uri.OriginalString}");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await requestMessage.SendAsync();

            var content = await response.Content.ReadAsStringAsync();

            this._testOutputHelper.WriteLine(content);
            Assert.True(response.IsSuccessStatusCode, "The expected success status code is not here.");
        }

        readonly ITestOutputHelper _testOutputHelper;
        readonly RestApiMetadata _restApiMetadata;
    }
}