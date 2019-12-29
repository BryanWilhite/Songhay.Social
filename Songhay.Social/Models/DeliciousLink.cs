using System;

namespace Songhay.Social.Models
{
    public class DeliciousLink
    {
        public Uri Href { get; set; }
        public DateTime AddDate { get; set; }
        public bool IsPrivate { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }
        public string DD { get; set; }
    }
}