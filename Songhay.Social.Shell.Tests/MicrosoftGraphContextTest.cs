using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using System;
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

            restApiMetadata = meta.RestApiMetadataSet.TryGetValueWithKey("MicrosoftGraph", throwException: true);
            var appId = restApiMetadata.ClaimsSet.TryGetValueWithKey("applicationId", throwException: true);
            app = new PublicClientApplication(appId);
        }

        [Ignore("This is an experimental test that should be run manually.")]
        [TestMethod]
        public async Task ShouldAcquireTokenAndListAppUser()
        {
            var scopes = restApiMetadata.ClaimsSet.TryGetValueWithKey("scopes", throwException: true).Split(',');
            AuthenticationResult authenticationResult = null;
            try
            {
                await app.AcquireTokenSilentAsync(scopes, app.Users.FirstOrDefault());
            }
            catch (MsalUiRequiredException ex)
            {
                this.TestContext.WriteLine(ex.Message);
                this.TestContext.WriteLine($"Cannot authenticate silently...");

                try
                {
                    authenticationResult = await app.AcquireTokenAsync(scopes);
                }
                catch(NotImplementedException ex2)
                {
                    this.TestContext.WriteLine(ex2.StackTrace);
                    Assert.Inconclusive("The Login Screen we see on full .NET Framework is not supported on .NET Core");
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

            var request = new HttpRequestMessage(HttpMethod.Get, restApiMetadata.ApiBase);
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
        static RestApiMetadata restApiMetadata;

    }
}
