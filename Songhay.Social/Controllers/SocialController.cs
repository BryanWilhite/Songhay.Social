using Microsoft.AspNetCore.Mvc;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.ModelContext;
using System;
using System.Collections.Generic;
using System.Net;

namespace Songhay.Social.Controllers
{
    [Route("api/[controller]")]
    public class SocialController : Controller
    {
        public SocialController(ProgramMetadata metadata)
        {
            this._metadata = metadata;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ICollection<Models.TwitterFavorite>), (int)HttpStatusCode.OK)]
        [Route("twitter-favorites")]
        public IActionResult GetTwitterFavorites()
        {
            var restApiMetadata = this._metadata.RestApiMetadataSet["SocialTwitter"];
            var profileImageBaseUri = new Uri(restApiMetadata.ClaimsSet["TwitterProfileImageRoot"], UriKind.Absolute);

            var authorizer = SocialContext.GetTwitterCredentialsAndAuthorizer(restApiMetadata.ClaimsSet.ToNameValueCollection());
            var favorites = SocialContext.GetTwitterFavorites(authorizer, profileImageBaseUri);
            return this.Ok(favorites);
        }

        ProgramMetadata _metadata;
    }
}
