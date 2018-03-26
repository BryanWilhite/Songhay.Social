using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

            var restApiMetadata = meta.RestApiMetadataSet.TryGetValueWithKey("MicrosoftGraphCommon", throwException: true);
            var appId = restApiMetadata.ClaimsSet.TryGetValueWithKey("applicationId", throwException: true);
            app = new PublicClientApplication(appId, restApiMetadata.ApiBase.OriginalString, TokenCacheHelper.GetUserCache());

            officeRestApiMetadata = meta.RestApiMetadataSet.TryGetValueWithKey("MicrosoftGraphSonghayOffice", throwException: true);
        }

        [TestMethod]
        [TestProperty("scopes", "user.read")]
        public async Task ShouldAcquireTokenAndListAppUser()
        {
            #region test properties:

            var scopes = this.TestContext.Properties["scopes"].ToString().Split(',');

            #endregion

            AuthenticationResult authenticationResult = null;
            try
            {
                authenticationResult = await app.AcquireTokenSilentAsync(scopes, app.Users.FirstOrDefault());
            }
            catch (MsalUiRequiredException ex)
            {
                this.TestContext.WriteLine(ex.Message);
                this.TestContext.WriteLine($"Cannot authenticate silently...");

                try
                {
                    authenticationResult = await app.AcquireTokenAsync(scopes);
                }
                catch (MsalException ex2)
                {
                    this.TestContext.WriteLine(ex2.Message);
                }
            }

            Assert.IsTrue(app.Users.Any(), "The expected user(s) is not here.");
            app.Users.ForEachInEnumerable(i => this.TestContext.WriteLine($"user: {i.Name}"));

            Assert.IsNotNull(authenticationResult, "The expected authentication result is not here.");
            this.TestContext.WriteLine($"{nameof(authenticationResult.AccessToken)}: {authenticationResult.AccessToken}");
            this.TestContext.WriteLine($"{nameof(authenticationResult.User.Name)}: {authenticationResult.User.Name}");
            this.TestContext.WriteLine($"{nameof(authenticationResult.User.DisplayableId)}: {authenticationResult.User.DisplayableId}");
            this.TestContext.WriteLine($"{nameof(authenticationResult.ExpiresOn)}: {authenticationResult.ExpiresOn.ToLocalTime()}");

            var request = new HttpRequestMessage(HttpMethod.Get, officeRestApiMetadata.ApiBase);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
            var response = await httpClient.SendAsync(request);
            Assert.IsTrue(response.IsSuccessStatusCode, "The expected success status code is not here.");

            var content = await response.Content.ReadAsStringAsync();
            this.TestContext.WriteLine($"token: {content}");
        }

        [TestMethod]
        public void ShouldSignOutUser()
        {
            try
            {
                app.Remove(app.Users.FirstOrDefault());
            }
            catch (MsalException ex)
            {
                this.TestContext.WriteLine(ex.Message);
            }

            Assert.IsFalse(app.Users.Any(), "User(s) are not expected.");
        }

        static readonly HttpClient httpClient;

        static PublicClientApplication app;
        static RestApiMetadata officeRestApiMetadata;

    }
}
