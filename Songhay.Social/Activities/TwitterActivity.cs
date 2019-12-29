using LinqToTwitter;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Extensions;
using Songhay.Social.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Songhay.Social.Activities
{
    public class TwitterActivity : IActivity
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

        public static IEnumerable<TwitterStatus> GetTwitterStatuses(IAuthorizer authorizer, Uri profileImageBaseUri)
        {
            var data = new List<Status>();
            using (var context = new TwitterContext(authorizer))
            {
                var ids = context.ToFavoriteStatusIds(count: 50);
                ids.Partition(10).ForEachInEnumerable(i =>
                {
                    data.AddRange(context.ToStatuses(i, TweetMode.Extended, includeEntities: true));
                });
            }

            if ((data == null) || !data.Any()) return Enumerable.Empty<TwitterStatus>();

            var favorites = data.Select(i => new TwitterStatus(i, profileImageBaseUri));
            return favorites;
        }

        public string DisplayHelp(ProgramArgs args)
        {
            throw new NotImplementedException();
        }

        public void Start(ProgramArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
