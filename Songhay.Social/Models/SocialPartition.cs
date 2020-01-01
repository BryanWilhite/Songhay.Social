using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Songhay.Social.Models
{
    public class SocialPartition
    {
        public string GroupName { get; set; }

        public int PartitionOrdinal { get; set; }

        public IEnumerable<JObject> Data { get; set; }
    }
}
