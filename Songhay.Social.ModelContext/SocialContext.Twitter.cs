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

        public static ICollection<TwitterFavorite> GetTwitterFavorites(IAuthorizer authorizer, Uri profileImageBaseUri)
        {
            var ctx = new TwitterContext(authorizer);

            #region functional members:

            Func<Favorites, bool> isFavoriteType = i => i.Type == FavoritesType.Favorites;
            Func<Favorites, int, bool> isLimitedTo = (i, count) => i.Count == count;
            Func<Favorites, bool> isIncludingEntities = (i) => i.IncludeEntities == true;

            #endregion

            var data = ctx.Favorites
                .Where(i => isFavoriteType(i) && isLimitedTo(i, 50) && isIncludingEntities(i))
                .ToList();
            if (!data.Any()) throw new TwitterQueryException("No items were found.");

            var favorites = data.Select(i => new TwitterFavorite(i, profileImageBaseUri)).ToList();
            return favorites;
        }
    }
}
