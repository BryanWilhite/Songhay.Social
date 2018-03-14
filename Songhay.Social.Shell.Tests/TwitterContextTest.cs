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
using System.Net;

namespace Songhay.Social.Shell.Tests
{
    [TestClass]
    public class TwitterContextTest
    {
        /// <summary>
        /// Initializes the test.
        /// </summary>
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

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [Ignore("This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [TestCategory("Integration")]
        [TestProperty("friendshipType", "FollowersList")]
        [TestProperty("pageSize", "10")] //increasing this value might exceed quota
        [TestProperty("screenName", "KinteSpace")]
        [TestMethod]
        public void ShouldSearchByScreenNameAndFriendshipType()
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
        [TestMethod]
        [TestProperty("screenNameList", "pluralsight,jongalloway")]
        public void ShouldSearchByScreenNameList()
        {
            #region test properties:

            var screenNameList = this.TestContext.Properties["screenNameList"].ToString();

            #endregion

            using (var ctx = new TwitterContext(this._authorizer))
            {
                var query = ctx.User
                    .Where(i => i.Type == UserType.Lookup)
                    .Where(i => i.ScreenNameList == screenNameList);

                var users = query.ToArray();
                Assert.IsNotNull(users, "The expected user set is not here.");
                Assert.IsTrue(users.Any(), "The expected users are not here.");

                users.ForEachInEnumerable(i => this.TestContext.WriteLine($"{i.ScreenName}"));
            }
        }

        [Ignore("This test runs against a rate-limited API so it should not be run automatically/regularly.")]
        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("count", "50")]
        public void ShouldSearchForFavoritesByCount()
        {
            #region test properties:

            var count = Convert.ToInt32(this.TestContext.Properties["count"]);

            #endregion

            using (var ctx = new TwitterContext(this._authorizer))
            {
                var query = ctx.Favorites.Where(i =>
                    (i.Type == FavoritesType.Favorites) &&
                    (i.IncludeEntities == false) &&
                    (i.Count == count));

                var favorites = query.ToArray();
                Assert.IsNotNull(favorites, "The expected favorite set is not here.");
                Assert.IsTrue(favorites.Any(), "The expected favorites are not here.");

                favorites.ForEachInEnumerable(i => this.TestContext.WriteLine($"{i.ScreenName}"));
            }
        }

        IAuthorizer _authorizer;
    }
}
