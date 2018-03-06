using LinqToTwitter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
            var data = new OpenAuthorizationData();

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

        [TestCategory("Integration")]
        [TestMethod]
        public void ShouldUseLinqToTwitterToFindInactiveAccounts()
        {
            //https://linqtotwitter.codeplex.com/wikipage?title=Showing%20Friends
            using (var ctx = new TwitterContext(this._authorizer))
            {
                long cursor = -1;
                do
                {
                    var friendship = ctx.Friendship
                        .Where(i => i.Type == FriendshipType.FollowersList)
                        .Where(i => i.ScreenName == "KinteSpace")
                        .Where(i => i.Count == 1000)
                        .Where(i => i.Cursor == cursor)
                        .Single();

                    if (friendship == null) break;
                    if (friendship.Users == null) break;
                    if (friendship.CursorMovement == null) break;

                    cursor = friendship.CursorMovement.Next;

                    friendship.Users.ForEachInEnumerable(i => this.TestContext.WriteLine("{0}", i.ScreenNameResponse));

                } while (cursor != 0);
            }
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void ShouldUseLinqToTwitterToReadFavoritesWithSingleUserAuthorizer()
        {
            using (var ctx = new TwitterContext(this._authorizer))
            {
                var query = ctx.Favorites
                    .Where(i => i.Type == FavoritesType.Favorites)
                    .Where(i => i.IncludeEntities == false)
                    .Where(i => i.Count == 50)
                    ;

                var favorites = query.ToList();
                Assert.IsNotNull(favorites, "The expected favorites are not here.");

                var count = favorites.Count();
                Assert.IsTrue(count > 0, "The expected count is not here");

                var userNames = new[] { "pluralsight", "jongalloway" };
                var query2 = ctx.User
                    .Where(i => i.Type == UserType.Lookup)
                    .Where(i => i.ScreenNameList == string.Join(",", userNames));

                var user = query2.ToList();
                Assert.IsNotNull(user, "The expected user is not here.");
            }
        }

        [TestCategory("Integration")]
        [TestMethod]
        [TestProperty("profileImageFolder", @"AzureBlobStorage-songhay\shared-social-twitter\")]
        [TestProperty("screenNames", "BryanWilhite,Kintespace")]
        public void ShouldUseLinqToTwitterToWriteProfileImages()
        {
            var projectsRoot = this.TestContext.ShouldGetAssemblyDirectoryInfo(this.GetType())
                ?.Parent
                ?.Parent
                ?.Parent
                ?.FullName;
            this.TestContext.ShouldFindDirectory(projectsRoot);

            #region test properties:

            var profileImageFolder = this.TestContext.Properties["profileImageFolder"].ToString();
            profileImageFolder = Path.Combine(projectsRoot, profileImageFolder);
            this.TestContext.ShouldFindDirectory(profileImageFolder);

            var screenNames = this.TestContext.Properties["screenNames"].ToString().Split(',');
            Assert.IsTrue(screenNames.Any(), "The expected screen names are not here.");

            #endregion

            using (var ctx = new TwitterContext(this._authorizer))
            {
                var favorites = ctx.Favorites
                    .Where(i => i.Type == FavoritesType.Favorites)
                    .Where(i => i.IncludeEntities == false)
                    .Where(i => i.Count == 50)
                    .ToList();

                var count = favorites.Count();
                Assert.IsTrue(count > 0, "The expected favorites count is not here");

                var usersFromFavorites = favorites.Select(i => i.User).ToList();

                count = usersFromFavorites.Count();
                Assert.IsTrue(count > 0, "The expected favorites user count is not here");

                var usersFromFollowing = new List<User>();
                screenNames.ForEachInEnumerable(screenName =>
                {
                    var users = ctx.Friendship
                        .Where(i => i.Type == FriendshipType.FriendsList)
                        .Where(i => i.ScreenName == screenName)
                        .Where(i => i.Count == 500)
                        .SelectMany(i => i.Users)
                        .ToArray();
                    usersFromFollowing.AddRange(users);
                });

                count = usersFromFollowing.Count();
                Assert.IsTrue(count > 0, "The expected follower count is not here");

                var profileImages = usersFromFavorites.Union(usersFromFollowing)
                    .Select(i => new
                    {
                        ScreenName = i.ScreenNameResponse,
                        ProfileImageUrl = i.ProfileImageUrl
                    })
                    .Distinct()
                    .ToList();

                profileImages.ForEachInEnumerable(i =>
                {
                    var uri = new Uri(i.ProfileImageUrl, UriKind.Absolute);
                    var target = profileImageFolder + i.ScreenName + "." + uri.Segments.Last().Split('.').Last().ToLower();
                    this.TestContext.WriteLine("writing {0}...", target);
                    WebRequest.CreateHttp(uri).DownloadToFile(target);
                });
            }
        }

        IAuthorizer _authorizer;
    }
}
