using LinqToTwitter;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Xml;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Songhay.Social.Models;

namespace Songhay.Social.ModelContext
{
    public static class SocialContext
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

        public static ICollection<DeliciousLink> LoadDeliciousLinks(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new NullReferenceException("The expected path is not here.");
            if (!File.Exists(path)) throw new FileLoadException("The expected file is not here.");
            var lines = File.ReadAllLines(path);
            var deliciousLinks = new List<DeliciousLink>();
            lines.Select((o, i) => new { Line = o, LineNumber = i }).ForEachInEnumerable(anonym =>
            {
                if (anonym.Line.StartsWith("<DT>"))
                {
                    var line = anonym.Line.Remove(0, 4)
                        .Replace("&amp;", "&")
                        .Replace("&", "&amp;");
                    var innerXml = HtmlUtility.GetInnerXml(line, "A");
                    line = line.Replace(innerXml, XmlUtility.ExpandSpecialChars(innerXml));
                    var A = XElement.Parse(line);
                    var link = new DeliciousLink
                    {
                        AddDate = FrameworkTypeUtility.ConvertDateTimeFromUnixTime(Convert.ToDouble(A.Attribute("ADD_DATE").Value)),
                        Href = new Uri(A.Attribute("HREF").Value, UriKind.Absolute),
                        IsPrivate = FrameworkTypeUtility.ParseBoolean(A.Attribute("PRIVATE").Value).GetValueOrDefault(),
                        Tags = A.Attribute("TAGS").Value,
                        Title = A.Value
                    };
                    deliciousLinks.Add(link);
                }
                else if (anonym.Line.StartsWith("<DD>"))
                {
                    var previousLink = deliciousLinks.Last();
                    previousLink.DD = anonym.Line.Remove(0, 4);
                }
            });

            return deliciousLinks.Take(100).ToList();
        }

        public static ICollection<DeliciousLink> LoadDeliciousLinks(string path, string tags, string searchText)
        {
            var links = LoadDeliciousLinks(path)
                .Where(i =>
                {
                    if (string.IsNullOrEmpty(tags) && string.IsNullOrEmpty(i.Tags)) return true;
                    else if (string.IsNullOrEmpty(tags)) return true;
                    else if (string.IsNullOrEmpty(i.Tags)) return false;
                    else
                    {
                        var test = false;
                        tags.Split(',').ForEachInEnumerable(j => test = i.Tags.Contains(j));
                        return test;
                    }
                })
                .Where(i =>
                {
                    if (string.IsNullOrEmpty(searchText)) return true;
                    else
                    {
                        var test = !string.IsNullOrEmpty(i.Title) && i.Title.Contains(searchText) ||
                            (i.Href != null) && i.Href.AbsoluteUri.Contains(searchText) ||
                            !string.IsNullOrEmpty(i.DD) && (i.DD.Contains(searchText));
                        return test;
                    }
                });
            return links.ToList();
        }
    }
}
