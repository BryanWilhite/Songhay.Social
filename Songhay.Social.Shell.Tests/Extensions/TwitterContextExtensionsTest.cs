using System;
using System.IO;
using System.Linq;
using LinqToTwitter;
using Microsoft.Extensions.Configuration;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests.Extensions
{
    public class TwitterContextExtensionsTest
    {
        public TwitterContextExtensionsTest(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;

            var projectRoot = ProgramAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
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
            this._testOutputHelper.WriteLine($"{meta}");

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

        [Theory(Skip = "This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [InlineData(50)]
        public void ShouldQueryFavoritesByCount(int count)
        {
            using(var context = new TwitterContext(this._authorizer))
            {
                var favorites = context.ToFavorites(count, includeEntities : false);
                Assert.True(favorites.Any(), "The expected favorites are not here.");
                favorites.ForEachInEnumerable(i => this._testOutputHelper.WriteLine($@"
{nameof(i.User.ScreenNameResponse)}: {i.User.ScreenNameResponse}
{nameof(i.ID)}: {i.ID}
{nameof(i.StatusID)}: {i.StatusID}
{nameof(i.Text)}: {i.Text}
{nameof(i.FullText)}: {i.FullText}
"));
            }
        }

        [Theory(Skip = "This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [InlineData("973322161426436097,964573153082064896,964534393464283137,963909405053018112,963841864011886592,963599531370844160,963291791448510464,963277605834248197,962749324126863360,962683197812338688")]
        public void ShouldQueryStatusesByStatusIds(string statusIdString)
        {
            var statusIds = statusIdString
                .Split(',')
                .Select(i => Convert.ToUInt64(i));

            using(var context = new TwitterContext(this._authorizer))
            {
                var statuses = context.ToStatuses(statusIds, TweetMode.Extended, includeEntities : true);
                Assert.NotNull(statuses);
                Assert.True(statuses.Any(), "The expected statuses are not here.");
                statuses.ForEachInEnumerable(i =>
                {

                    this._testOutputHelper.WriteLine($@"
{nameof(i.ID)}: {i.ID}
{nameof(i.StatusID)}: {i.StatusID}
{nameof(i.User.ScreenNameResponse)}: {i.User.ScreenNameResponse}
{nameof(i.Text)}: {i.Text}
{nameof(i.FullText)}: {i.FullText}
");
                });
            }
        }

        readonly IAuthorizer _authorizer;
        readonly ITestOutputHelper _testOutputHelper;
    }
}