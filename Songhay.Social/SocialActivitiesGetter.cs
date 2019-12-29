using Songhay.Models;
using System;
using System.Collections.Generic;

namespace Songhay.Social
{
    public class SocialActivitiesGetter : ActivitiesGetter
    {
        public SocialActivitiesGetter(string[] args) : base(args)
        {
            this.LoadActivities(new Dictionary<string, Lazy<IActivity>>
            {
                {
                    nameof(Activities.DeliciousActivity),
                    new Lazy<IActivity>(() => new Activities.DeliciousActivity())
                },
                {
                    nameof(Activities.TwitterActivity),
                    new Lazy<IActivity>(() => new Activities.TwitterActivity())
                }
            });
        }
    }
}
