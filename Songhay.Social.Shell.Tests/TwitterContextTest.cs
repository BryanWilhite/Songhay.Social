using LinqToTwitter;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.ModelContext.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Songhay.Social.Shell.Tests
{
    [TestClass]
    public class TwitterContextTest
    {
        static TwitterContextTest()
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
"));
            }
        }

        [Ignore("This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [TestCategory("Integration")]
        [TestProperty("friendshipType", "FollowersList")]
        [TestProperty("pageSize", "10")] //increasing this value might exceed quota
        [TestProperty("screenName", "KinteSpace")]
        [TestMethod]
        public void ShouldQueryFriendshipByScreenName()
        {
            #region test properties:

            var friendshipType = Enum.Parse<FriendshipType>(this.TestContext.Properties["friendshipType"].ToString());
            var pageSize = Convert.ToInt32(this.TestContext.Properties["pageSize"]);
            var screenName = this.TestContext.Properties["screenName"].ToString();

            #endregion

            using (var ctx = new TwitterContext(this._authorizer))
            {
                long cursor = -1;
                do
                {
                    var friendship = ctx.Friendship
                        .Where(i =>
                            (i.Type == friendshipType) &&
                            (i.ScreenName == screenName) &&
                            (i.Count == pageSize) &&
                            (i.Cursor == cursor))
                        .Single();

                    if (friendship == null) break;
                    if (friendship.Users == null) break;
                    if (friendship.CursorMovement == null) break;

                    cursor = friendship.CursorMovement.Next;

                    friendship.Users.ForEachInEnumerable(i => this.TestContext.WriteLine($"{i.ScreenNameResponse}"));

                } while (cursor != 0);
            }
        }

        [Ignore("This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("statusId", "964573153082064896")]
        public void ShouldQueryStatusByStatusId()
        {
            #region test properties:

            var statusId = Convert.ToUInt64(this.TestContext.Properties["statusId"]);

            #endregion

            using (var context = new TwitterContext(this._authorizer))
            {
                var query = context.Status.Where(i =>
                    (i.Type == StatusType.Show) &&
                    (i.TweetMode == TweetMode.Extended) &&
                    (i.IncludeEntities == true) &&
                    (i.ID == statusId));

                var status = query.Single();
                Assert.IsNotNull(status, "The expected status is not here.");
                this.TestContext.WriteLine($@"
{nameof(status.ID)}: {status.ID}
{nameof(status.StatusID)}: {status.StatusID}
{nameof(status.User.ScreenNameResponse)}: {status.User.ScreenNameResponse}
{nameof(status.Text)}: {status.Text}
{nameof(status.FullText)}: {status.FullText}
");
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

        [Ignore("This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [TestMethod]
        [TestProperty("screenNameList", "pluralsight,jongalloway")]
        public void ShouldQueryUsersByScreenNameList()
        {
            #region test properties:

            var screenNameList = this.TestContext.Properties["screenNameList"].ToString();

            #endregion

            using (var context = new TwitterContext(this._authorizer))
            {
                var query = context.User.Where(i =>
                    (i.Type == UserType.Lookup) &&
                    (i.ScreenNameList == screenNameList));

                var users = query.ToArray();
                Assert.IsNotNull(users, "The expected user set is not here.");
                Assert.IsTrue(users.Any(), "The expected users are not here.");

                users.ForEachInEnumerable(i => this.TestContext.WriteLine($"{i.ScreenNameResponse}"));
            }
        }

        [Ignore("This test is intended to run manually.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("profileImageFolder", @"azure-storage-accounts\songhay\shared-social-twitter\")]
        [TestProperty("screenNameList", "BryanWilhite,Kintespace")]
        [TestProperty("count", "50")]
        public async Task ShouldWriteProfileImages()
        {
            var root = this.TestContext.ShouldGetAssemblyDirectoryParent(this.GetType(), expectedLevels: 5);

            #region test properties:

            var profileImageFolder = Path.Combine(root, this.TestContext.Properties["profileImageFolder"].ToString());
            this.TestContext.ShouldFindDirectory(profileImageFolder);

            var screenNameList = this.TestContext.Properties["screenNameList"].ToString().Split(',');
            var count = Convert.ToInt32(this.TestContext.Properties["count"]);

            #endregion

            using (var context = new TwitterContext(this._authorizer))
            {
                var favorites = context.ToFavorites(count, includeEntities: false);
                Assert.IsTrue(favorites.Any(), "The expected favorites are not here");

                var usersFromFavorites = favorites.Select(i => i.User).ToList();
                Assert.IsTrue(usersFromFavorites.Any(), "The expected favorites users are not here");

                var usersFromFollowing = new List<User>();
                screenNameList.ForEachInEnumerable(screenName =>
                {
                    var users = context.ToUsersByScreenName(screenName, count: 500);
                    usersFromFollowing.AddRange(users);
                });

                Assert.IsTrue(usersFromFollowing.Any(), "The expected followers are not here");

                var profileImages = usersFromFavorites.Union(usersFromFollowing)
                    .Select(i => new
                    {
                        ScreenName = i.ScreenNameResponse,
                        i.ProfileImageUrl
                    })
                    .Distinct()
                    .ToArray();

                foreach (var i in profileImages)
                {
                    var uri = new Uri(i.ProfileImageUrl, UriKind.Absolute);
                    var target = string.Concat(
                        profileImageFolder,
                        i.ScreenName, ".",
                        uri.Segments.Last().Split('.').Last().ToLower()
                        );
                    this.TestContext.WriteLine($"writing {target}...");
                    await httpClient.DownloadToFileAsync(uri, target);
                }
            }
        }

        static readonly HttpClient httpClient;

        IAuthorizer _authorizer;

    }
}
