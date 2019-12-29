using Songhay.Models;
using System;
using System.Collections.Generic;

namespace Songhay.Social.Extensions
{
    /// <summary>
    /// Extensions of <see cref="RestApiMetadata"/>
    /// </summary>
    public static class RestApiMetadataExtensions
    {
        /// <summary>
        /// Converts the <see cref="RestApiMetadata"/> to the twitter profile image root URI.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">metadata - The expected REST API metadata is not here.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The expected REST API claim is not here.</exception>
        public static Uri ToTwitterProfileImageRootUri(this RestApiMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata), "The expected REST API metadata is not here.");
            var location = metadata.ClaimsSet["TwitterProfileImageRoot"];
            var uri = new Uri(location, UriKind.Absolute);
            return uri;
        }
    }
}
