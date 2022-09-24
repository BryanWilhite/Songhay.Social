using Newtonsoft.Json.Linq;

namespace Songhay.Social.Models;

public class SocialPartition
{
    public string? GroupName { get; init; }

    public int? PartitionOrdinal { get; init; }

    public IEnumerable<JObject> Data { get; set; } = new List<JObject>();
}
