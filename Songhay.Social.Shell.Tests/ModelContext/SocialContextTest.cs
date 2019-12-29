using LinqToTwitter;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.ModelContext;
using Songhay.Social.Extensions;
using System;
using System.Linq;
using System.Net.Http;

namespace Songhay.Social.Shell.Tests.ModelContext
{
    [TestClass]
    public class SocialContextTest
    {
        static SocialContextTest()
        {
            httpClient = new HttpClient();
        }

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            var targetDirectoryInfo = this.TestContext.ShouldGetConventionalProjectDirectoryInfo(this.GetType());
            var basePath = targetDirectoryInfo.FullName;
            meta = new ProgramMetadata();
            var configuration = this.TestContext.ShouldLoadConfigurationFromConventionalProject(this.GetType(), b =>
            {
                b.AddJsonFile("./app-settings.songhay-system.json", optional: false, reloadOnChange: false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), meta);
            this.TestContext.WriteLine($"{meta}");

            var restApiMetadata = meta.ToSocialTwitterRestApiMetadata();

            var data = new OpenAuthorizationData(restApiMetadata.ClaimsSet.ToNameValueCollection());

            this._authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = data.ConsumerKey,
                    ConsumerSecret = data.ConsumerSecret,
                    OAuthToken = data.Token,
                    OAuthTokenSecret = data.TokenSecret
                }
            };
        }

        [TestMethod]
        public void ShouldGetTwitterStatuses()
        {
            var restApiMetadata = meta.ToSocialTwitterRestApiMetadata();
            var profileImageBaseUri = restApiMetadata.ToTwitterProfileImageRootUri();

            var statuses = SocialContext.GetTwitterStatuses(this._authorizer, profileImageBaseUri);
            Assert.IsTrue(statuses.Any(), "The expected statuses are not here.");

            statuses.ForEachInEnumerable(i =>
            {

                this.TestContext.WriteLine($@"
{nameof(i.ID)}: {i.ID}
{nameof(i.StatusID)}: {i.StatusID}
{nameof(i.User.ScreenNameResponse)}: {i.User.ScreenNameResponse}
{nameof(i.Text)}: {i.Text}
{nameof(i.FullText)}: {i.FullText}
");
            });
        }

        static readonly HttpClient httpClient;
        static ProgramMetadata meta;

        IAuthorizer _authorizer;
    }
}
