using Microsoft.AspNetCore.Mvc;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.ModelContext;
using System;
using System.Collections.Generic;
using Songhay.Social.ModelContext.Extensions;
using System.Net;
using LinqToTwitter;

namespace Songhay.Social.Controllers
{
    /// <summary>
    /// Controls social media API(s)
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]/v1")]
    public class SocialController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialController"/> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public SocialController(ProgramMetadata metadata)
        {
            var restApiMetadata = metadata.ToSocialTwitterRestApiMetadata();
            this.profileImageBaseUri = restApiMetadata.ToTwitterProfileImageRootUri();
            this.twitterAuthorizer = SocialContext.GetTwitterCredentialsAndAuthorizer(restApiMetadata.ClaimsSet.ToNameValueCollection());
        }

        /// <summary>
        /// Gets the Twitter favorites.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<Models.TwitterFavorite>), (int)HttpStatusCode.OK)]
        [Route("twitter-favorites")]
        public IActionResult GetTwitterFavorites()
        {
            var favorites = SocialContext.GetTwitterStatuses(this.twitterAuthorizer, this.profileImageBaseUri);
            return this.Ok(favorites);
        }

        readonly IAuthorizer twitterAuthorizer;
        readonly Uri profileImageBaseUri;
    }
}
