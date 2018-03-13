using Songhay.Models;
using System;

namespace Songhay.Social.ModelContext.Extensions
{
    /// <summary>
    /// Extensions of <see cref="ProgramMetadata"/>
    /// </summary>
    public static class ProgramMetadataExtensions
    {
        /// <summary>
        /// Converts <see cref="ProgramMetadata"/>
        /// to <see cref="RestApiMetadata"/>
        /// for social twitter.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <returns></returns>
        public static RestApiMetadata ToSocialTwitterRestApiMetadata(this ProgramMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata), "The expected Program metadata is not here.");

            var restApiMetadata = metadata.RestApiMetadataSet["SocialTwitter"];

            return restApiMetadata;
        }
    }
}
