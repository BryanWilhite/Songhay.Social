using LinqToTwitter;
using Songhay.Models;
using Songhay.Social.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Songhay.Social.ModelContext
{
    public static partial class SocialContext
    {
        public static IAuthorizer GetTwitterCredentialsAndAuthorizer(NameValueCollection configurationData)
        {
            var data = new OpenAuthorizationData(configurationData);

            var authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = data.ConsumerKey,
                    ConsumerSecret = data.ConsumerSecret,
                    OAuthToken = data.Token,
                    OAuthTokenSecret = data.TokenSecret
                },
            };

            return authorizer;
        }

        public static IEnumerable<TwitterFavorite> GetTwitterFavorites(IAuthorizer authorizer, Uri profileImageBaseUri)
        {
            var ctx = new TwitterContext(authorizer);

            var data = ctx.Favorites
                .Where(i =>
                    (i.Type == FavoritesType.Favorites) &&
                    (i.Count == 50) &&
                    (i.IncludeEntities == true))
                .ToArray();

            if ((data == null) || !data.Any()) return Enumerable.Empty<TwitterFavorite>();

            var favorites = data.Select(i => new TwitterFavorite(i, profileImageBaseUri));
            return favorites;
        }
    }
}
