using Microsoft.AspNetCore.Mvc;
using Songhay.Extensions;
using Songhay.Models;
using System;
using System.Collections.Generic;
using Songhay.Social.Extensions;
using System.Net;
using LinqToTwitter;
using Songhay.Social.Activities;

namespace Songhay.Social.Web.Controllers
{
    /// <summary>
    /// Controls social media API(s)
    /// </summary>
    /// <seealso cref="Controller" />
    [Route("[controller]/v1")]
    public class TwitterController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterController"/> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public TwitterController(ProgramMetadata metadata)
        {
            var restApiMetadata = metadata.ToSocialTwitterRestApiMetadata();
            this.profileImageBaseUri = restApiMetadata.ToTwitterProfileImageRootUri();
            this.twitterAuthorizer = TwitterActivity.GetTwitterCredentialsAndAuthorizer(restApiMetadata.ClaimsSet.ToNameValueCollection());
        }

        /// <summary>
        /// Gets the Twitter favorites.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<Models.TwitterStatus>), (int)HttpStatusCode.OK)]
        [Route("statuses")]
        public IActionResult GetTwitterStatuses()
        {
            var statuses = TwitterActivity.GetTwitterStatuses(this.twitterAuthorizer, this.profileImageBaseUri);
            return this.Ok(statuses);
        }

        readonly IAuthorizer twitterAuthorizer;
        readonly Uri profileImageBaseUri;
    }
}
