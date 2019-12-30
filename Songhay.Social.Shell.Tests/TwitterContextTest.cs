using LinqToTwitter;
using Microsoft.Extensions.Configuration;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests
{
    public class TwitterContextTest
    {
        public TwitterContextTest(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;

            var projectRoot = FrameworkAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var basePath = projectInfo.Parent.FindDirectory("Songhay.Social.Web").FullName;
            var meta = new ProgramMetadata();
            var configuration = ProgramUtility.LoadConfiguration(basePath, b =>
            {
                b.AddJsonFile("./app-settings.songhay-system.json", optional: false, reloadOnChange: false);
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

        [Theory]
        [InlineData(
            "FollowersList",
            10, //increasing this value might exceed quota
            "KinteSpace")]
        public void ShouldQueryFriendshipByScreenName(string friendshipTypeString, int pageSize, string screenName)
        {
            var friendshipType = Enum.Parse<FriendshipType>(friendshipTypeString);

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

                    friendship.Users.ForEachInEnumerable(i => this._testOutputHelper.WriteLine($"{i.ScreenNameResponse}"));

                } while (cursor != 0);
            }
        }

        [Theory]
        [InlineData(964573153082064896)]
        public void ShouldQueryStatusByStatusId(ulong statusId)
        {
            using (var context = new TwitterContext(this._authorizer))
            {
                var query = context.Status.Where(i =>
                    (i.Type == StatusType.Show) &&
                    (i.TweetMode == TweetMode.Extended) &&
                    (i.IncludeEntities == true) &&
                    (i.ID == statusId));

                var status = query.Single();
                Assert.NotNull(status);
                this._testOutputHelper.WriteLine($@"
{nameof(status.ID)}: {status.ID}
{nameof(status.StatusID)}: {status.StatusID}
{nameof(status.User.ScreenNameResponse)}: {status.User.ScreenNameResponse}
{nameof(status.Text)}: {status.Text}
{nameof(status.FullText)}: {status.FullText}
");
            }
        }

        [Theory]
        [InlineData("pluralsight,jongalloway")]
        public void ShouldQueryUsersByScreenNameList(string screenNames)
        {
            using (var context = new TwitterContext(this._authorizer))
            {
                var query = context.User.Where(i =>
                    (i.Type == UserType.Lookup) &&
                    (i.ScreenNameList == screenNames));

                var users = query.ToArray();
                Assert.NotNull(users);
                Assert.True(users.Any(), "The expected users are not here.");

                users.ForEachInEnumerable(i => this._testOutputHelper.WriteLine($"{i.ScreenNameResponse}"));
            }
        }

        [Theory]
        [InlineData(@"azure-storage-accounts\songhay\shared-social-twitter\", "BryanWilhite,Kintespace", 50)]
        public async Task ShouldWriteProfileImages(string profileImageFolder, string screenNames, int count)
        {
            var root = FrameworkAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../../../");
            var rootInfo = new DirectoryInfo(root);

            profileImageFolder = rootInfo.ToCombinedPath(profileImageFolder);
            Assert.True(Directory.Exists(profileImageFolder));

            var screenNameList = screenNames.Split(',');

            using (var context = new TwitterContext(this._authorizer))
            {
                var favorites = context.ToFavorites(count, includeEntities: false);
                Assert.True(favorites.Any());

                var usersFromFavorites = favorites.Select(i => i.User).ToList();
                Assert.True(usersFromFavorites.Any());

                var usersFromFollowing = new List<User>();
                screenNameList.ForEachInEnumerable(screenName =>
                {
                    var users = context.ToUsersByScreenName(screenName, count: 500);
                    usersFromFollowing.AddRange(users);
                });

                Assert.True(usersFromFollowing.Any());

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
                    var message = new HttpRequestMessage(HttpMethod.Get, uri);
                    this._testOutputHelper.WriteLine($"writing {target}...");

                    var response = await message.SendAsync();
                    var data = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(target, data);
                }
            }
        }

        readonly IAuthorizer _authorizer;
        readonly ITestOutputHelper _testOutputHelper;

    }
}
