using Songhay.Abstractions;
using Songhay.Models;

namespace Songhay.Social;

public sealed class SocialActivitiesGetter : ActivitiesGetter
{
    public SocialActivitiesGetter(string[] args) : base(args)
    {
        LoadActivities(new Dictionary<string, Lazy<IActivity?>>
        {
            {
                nameof(Activities.DeliciousActivity),
                new Lazy<IActivity?>(() => new Activities.DeliciousActivity())
            },
            {
                nameof(Activities.IExcelDataReaderActivity),
                new Lazy<IActivity?>(() => new Activities.IExcelDataReaderActivity())
            },
            {
                nameof(Activities.UniformResourceActivity),
                new Lazy<IActivity?>(() => new Activities.UniformResourceActivity())
            }
        });
    }
}
