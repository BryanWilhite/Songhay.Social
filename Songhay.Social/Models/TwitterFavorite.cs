using LinqToTwitter;
using Songhay.Extensions;
using System;
using System.Linq;

namespace Songhay.Social.Models
{
    public class TwitterFavorite : Favorites
    {
        public TwitterFavorite(Favorites data, Uri baseUri)
        {
            if (data == null) throw new NullReferenceException("The expected Twitter data is not here.");

            var uri = new Uri(data.User.ProfileImageUrl, UriKind.Absolute);
            var lastSegment = uri.Segments.Last().Split('.').Last().ToLower();
            var uriBuilder = new UriBuilder(baseUri).WithPath($"{data.User.ScreenNameResponse}.{lastSegment}");

            this.ProfileImageUrl = uriBuilder.Uri.OriginalString;
            ProgramTypeUtility.SetProperties(data, this);
        }

        public string ProfileImageUrl { get; private set; }
    }
}