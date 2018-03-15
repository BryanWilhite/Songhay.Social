using LinqToTwitter;
using System.Collections.Generic;
using System.Linq;

namespace Songhay.Social.ModelContext.Extensions
{
    /// <summary>
    /// Extensions of <see cref="TwitterContext"/>
    /// </summary>
    public static class TwitterContextExtensions
    {
        /// <summary>
        /// Converts <see cref="TwitterContext"/>
        /// to <see cref="IEnumerable{Favorites}"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="count">The count.</param>
        /// <param name="includeEntities">if set to <c>true</c> [include entities].</param>
        /// <returns></returns>
        public static IEnumerable<Favorites> ToFavorites(this TwitterContext context, int count, bool includeEntities)
        {
            if (context == null) return Enumerable.Empty<Favorites>();

            return context
                .Favorites
                .Where(i =>
                    (i.Type == FavoritesType.Favorites) &&
                    (i.Count == count) &&
                    (i.IncludeEntities == includeEntities))
                .ToArray();
        }

        /// <summary>
        /// Converts <see cref="TwitterContext"/>
        /// to <see cref="IEnumerable{User}"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="screenName">Name of the screen.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static IEnumerable<User> ToUsersByScreenName(this TwitterContext context, string screenName, int count)
        {
            if (context == null) return Enumerable.Empty<User>();

            return context
                .Friendship
                .Where(i =>
                    (i.Type == FriendshipType.FriendsList) &&
                    (i.ScreenName == screenName) &&
                    (i.Count == count))
                .SelectMany(i => i.Users)
                .ToArray();
        }
    }
}
