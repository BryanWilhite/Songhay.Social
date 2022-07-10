#nullable enable

using System;
using System.Security.Cryptography;
using System.Text;
using Songhay.Models;

namespace Songhay.Social.Extensions
{
    /// <summary>
    /// Extensions of <see cref="OpenAuthorizationData"/>.
    /// </summary>
    public static class OpenAuthorizationDataExtensions
    {
        /// <summary>
        /// Gets the name of the twitter base URI with screen.
        /// </summary>
        /// <param name="twitterBaseUri">The twitter base URI.</param>
        /// <param name="screenName">Name of the screen.</param>
        public static Uri? GetTwitterBaseUriWithScreenName(this Uri? twitterBaseUri, string? screenName)
        {
            if (twitterBaseUri == null) return null;

            return string.IsNullOrWhiteSpace(screenName)
                ? null
                : new Uri(twitterBaseUri.OriginalString + "?screen_name=" + Uri.EscapeDataString(screenName));
        }

        /// <summary>
        /// Gets the name of the twitter base URI with screen.
        /// </summary>
        /// <param name="twitterBaseUri">The twitter base URI.</param>
        /// <param name="screenName">Name of the screen.</param>
        /// <param name="count">The count.</param>
        public static Uri? GetTwitterBaseUriWithScreenName(this Uri? twitterBaseUri, string? screenName, int count)
        {
            if (twitterBaseUri == null) return null;

            return string.IsNullOrWhiteSpace(screenName)
                ? null
                : new Uri(twitterBaseUri.OriginalString + $"?count={count}&screen_name={Uri.EscapeDataString(screenName)}");
        }

        /// <summary>
        /// Gets the twitter request header.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="twitterBaseUri">The twitter base URI.</param>
        /// <param name="screenName">Name of the screen.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        public static string? GetTwitterRequestHeader(this OpenAuthorizationData? data, Uri? twitterBaseUri,
            string? screenName, string httpMethod)
        {
            if (data == null) return null;
            if (twitterBaseUri == null) return null;
            if (string.IsNullOrWhiteSpace(screenName)) return null;

            var baseFormat =
                "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&screen_name={6}";

            var baseString = string.Format(
                baseFormat,
                data.ConsumerKey,
                data.Nonce,
                data.SignatureMethod,
                data.TimeStamp,
                data.Token,
                data.Version,
                Uri.EscapeDataString(screenName));

            baseString = string.Concat(httpMethod.ToUpper(), "&", Uri.EscapeDataString(twitterBaseUri.OriginalString), "&",
                Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(data.ConsumerSecret ?? string.Empty), "&",
                Uri.EscapeDataString(data.TokenSecret ?? string.Empty));

            using HMACSHA1 hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey));

            var signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));

            var headerFormat =
                "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                Uri.EscapeDataString(data.Nonce ?? string.Empty),
                Uri.EscapeDataString(data.SignatureMethod ?? string.Empty),
                Uri.EscapeDataString(data.TimeStamp ?? string.Empty),
                Uri.EscapeDataString(data.ConsumerKey ?? string.Empty),
                Uri.EscapeDataString(data.Token ?? string.Empty),
                Uri.EscapeDataString(signature),
                Uri.EscapeDataString(data.Version ?? string.Empty));

            return authHeader;
        }
    }
}
