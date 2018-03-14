using LinqToTwitter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using Songhay.Models;
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

        [Ignore("This test requires manual authorization so it should not be run on a CI/CD server.")]
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
                        .Where(i =>
                            (i.Type == FriendshipType.FollowersList) &&
                            (i.ScreenName == "KinteSpace") &&
                            (i.Count == 1000) &&
                            (i.Cursor == cursor))
                        .Single();

                    if (friendship == null) break;
                    if (friendship.Users == null) break;
                    if (friendship.CursorMovement == null) break;

                    cursor = friendship.CursorMovement.Next;

                    friendship.Users.ForEachInEnumerable(i => this.TestContext.WriteLine("{0}", i.ScreenNameResponse));

                } while (cursor != 0);
            }
        }

        [Ignore("This test requires manual authorization so it should not be run on a CI/CD server.")]
        [TestCategory("Integration")]
        [TestMethod]
        public void ShouldUseLinqToTwitterToReadFavoritesWithSingleUserAuthorizer()
        {
            using (var ctx = new TwitterContext(this._authorizer))
            {
                var query = ctx.Favorites.Where(i =>
                    (i.Type == FavoritesType.Favorites) &&
                    (i.IncludeEntities == false) &&
                    (i.Count == 50));

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

        IAuthorizer _authorizer;
    }
}
