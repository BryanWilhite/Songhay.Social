using LinqToTwitter;
using System.Collections.Generic;
using System.Linq;

namespace Songhay.Social.Extensions
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
                    (i.IncludeEntities == includeEntities) &&
                    (i.TweetMode == TweetMode.Extended))
                .ToArray();
        }

        /// <summary>
        /// Converts <see cref="TwitterContext"/>
        /// to <see cref="IEnumerable{ulong}"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static IEnumerable<ulong> ToFavoriteStatusIds(this TwitterContext context, int count)
        {
            if (context == null) return Enumerable.Empty<ulong>();

            return context
                .Favorites
                .Where(i =>
                    (i.Type == FavoritesType.Favorites) &&
                    (i.Count == count))
                .Select(i => i.StatusID)
                .ToArray();
        }

        /// <summary>
        /// Converts <see cref="TwitterContext"/>
        /// to <see cref="IEnumerable{Status}"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="statusIds">The status ids.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="includeEntities">if set to <c>true</c> [include entities].</param>
        /// <returns></returns>
        public static IEnumerable<Status> ToStatuses(this TwitterContext context, IEnumerable<ulong> statusIds, TweetMode mode, bool includeEntities)
        {
            if (context == null) return Enumerable.Empty<Status>();

            var ids = string.Join(",", statusIds);

            var query = context.Status.Where(i =>
                (i.Type == StatusType.Lookup) &&
                (i.TweetMode == mode) &&
                (i.IncludeEntities == includeEntities) &&
                (i.TweetIDs == ids));

            var statuses = query.ToArray();

            return statuses;
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
