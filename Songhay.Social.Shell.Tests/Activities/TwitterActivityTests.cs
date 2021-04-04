using LinqToTwitter;
using Microsoft.Extensions.Configuration;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Activities;
using Songhay.Social.Extensions;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests.Activities
{
    public class TwitterActivityTests
    {
        public TwitterActivityTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;

            var projectRoot = ProgramAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var basePath = projectInfo.Parent.FindDirectory("Songhay.Social.Web").FullName;
            _meta = new ProgramMetadata();
            var configuration = ProgramUtility.LoadConfiguration(basePath, b =>
            {
                b.AddJsonFile("./app-settings.songhay-system.json", optional: false, reloadOnChange: false);
                b.SetBasePath(basePath);
                return b;
            });
            configuration.Bind(nameof(ProgramMetadata), _meta);
            this._testOutputHelper.WriteLine($"{_meta}");

            var restApiMetadata = _meta.ToSocialTwitterRestApiMetadata();

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

        [Fact]
        public void ShouldGetTwitterStatuses()
        {
            var restApiMetadata = _meta.ToSocialTwitterRestApiMetadata();
            var profileImageBaseUri = restApiMetadata.ToTwitterProfileImageRootUri();

            var statuses = TwitterActivity.GetTwitterStatuses(this._authorizer, profileImageBaseUri);
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

        readonly IAuthorizer _authorizer;
        readonly ITestOutputHelper _testOutputHelper;
        readonly ProgramMetadata _meta;
    }
}
