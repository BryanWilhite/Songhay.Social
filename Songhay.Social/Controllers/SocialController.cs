using Microsoft.AspNetCore.Mvc;
using Songhay.Models;
using Songhay.Social.ModelContext;
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
            var set = this._metadata.RestApiMetadataSet["SocialTwitter"];
            var authorizer = SocialContext.GetTwitterCredentialsAndAuthorizer(configurationData: set);
            var favorites = SocialContext.GetTwitterFavorites(authorizer);
            return this.Ok(favorites);
        }

        ProgramMetadata _metadata;
    }
}
