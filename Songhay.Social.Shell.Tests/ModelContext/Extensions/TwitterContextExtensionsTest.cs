using LinqToTwitter;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.ModelContext.Extensions;
using System;
using System.Linq;
using System.Net.Http;


namespace Songhay.Social.Shell.Tests.ModelContext.Extensions
{
    /// <summary>
    /// Extensions of <see cref="TwitterContextExtensions"/>
    /// </summary>
    [TestClass]
    public class TwitterContextExtensionsTest
    {
        static TwitterContextExtensionsTest()
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

        [Ignore("This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("count", "50")]
        public void ShouldQueryFavoritesByCount()
        {
            #region test properties:

            var count = Convert.ToInt32(this.TestContext.Properties["count"]);

            #endregion

            using (var context = new TwitterContext(this._authorizer))
            {
                var favorites = context.ToFavorites(count, includeEntities: false);
                Assert.IsTrue(favorites.Any(), "The expected favorites are not here.");
                favorites.ForEachInEnumerable(i => this.TestContext.WriteLine($@"
{nameof(i.User.ScreenNameResponse)}: {i.User.ScreenNameResponse}
{nameof(i.ID)}: {i.ID}
{nameof(i.StatusID)}: {i.StatusID}
{nameof(i.Text)}: {i.Text}
{nameof(i.FullText)}: {i.FullText}
"));
            }
        }

        [Ignore("This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("statusIds", "973322161426436097,964573153082064896,964534393464283137,963909405053018112,963841864011886592,963599531370844160,963291791448510464,963277605834248197,962749324126863360,962683197812338688")]
        public void ShouldQueryStatusesByStatusIds()
        {
            #region test properties:

            var statusIds = this.TestContext.Properties["statusIds"]
                .ToString()
                .Split(',')
                .Select(i => Convert.ToUInt64(i));

            #endregion

            using (var context = new TwitterContext(this._authorizer))
            {
                var statuses = context.ToStatuses(statusIds, TweetMode.Extended, includeEntities: true);
                Assert.IsNotNull(statuses, "The expected status set is not here.");
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
        }

        static readonly HttpClient httpClient;

        IAuthorizer _authorizer;
    }
}
