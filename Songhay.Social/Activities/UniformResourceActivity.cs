using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Polly;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Extensions;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Songhay.Abstractions;

namespace Songhay.Social.Activities;

public class UniformResourceActivity : IActivityWithTask<string, string>
{
    static UniformResourceActivity()
    {
        TraceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();

        var retryCount = 3;

        retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount,
                (currentRetryNumber) =>
                {
                    TraceSource?.TraceInformation($"Attempt {currentRetryNumber} of {retryCount})...");

                    return TimeSpan.FromSeconds(3);
                },
                (exception, timeSpan) =>
                {
                    TraceSource?.TraceError(exception);
                    TraceSource?.TraceInformation($"Retrying in {timeSpan.Seconds} seconds...");
                }
            );
    }

    static readonly TraceSource TraceSource;

    public string DisplayHelp(ProgramArgs args)
    {
        args.WithDefaultHelpText();

        args.HelpSet.Remove(ProgramArgs.BasePath);
        args.HelpSet.Remove(ProgramArgs.BasePathRequired);
        args.HelpSet.Remove(ProgramArgs.OutputUnderBasePath);
        args.HelpSet.Remove(ProgramArgs.SettingsFile);

        return args.ToHelpDisplayText();
    }

    public void Start(ProgramArgs args)
    {
        var json = args.GetStringInput();

        var output = StartAsync(json).GetAwaiter().GetResult();

        args.WriteOutputToFile(output);
    }

    public async Task<string> StartAsync(string json)
    {
        TraceSource?.WriteLine($"{nameof(UniformResourceActivity)} starting...");

        var uris = JsonSerializer.Deserialize<string[]>(json);

        return await GenerateTwitterPartitionAsync(uris)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    internal async Task<string> GenerateTwitterPartitionAsync(string[] uris)
    {

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var htmlWeb = new HtmlWeb().WithChromeishUserAgent();

        var tasks = uris.Select(i => htmlWeb.ToSocialDataAsync(i, retryPolicy));

        await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);

        var jObjects = tasks.Select(i => i.Result);

        var jPartition = new JArray(jObjects);

        return jPartition.ToString();
    }

    internal static readonly AsyncPolicy retryPolicy;
}