using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Extensions;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Songhay.Social.Activities
{
    public class UniformResourceActivity : IActivityWithOutput<string, string>
    {
        static UniformResourceActivity() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();

        static TraceSource traceSource;

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

            var output = this.StartForOutput(json);

            args.WriteOutputToFile(output);
        }

        public string StartForOutput(string json)
        {
            traceSource?.WriteLine($"{nameof(UniformResourceActivity)} starting...");

            var uris = JsonSerializer.Deserialize<string[]>(json);

            return this.GenerateTwitterPartitions(uris);
        }

        internal string GenerateTwitterPartitions(string[] uris)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var htmlWeb = new HtmlWeb().WithChromeishUserAgent();

            var jPartition = new JArray(uris
                .Select(htmlWeb.ToSocialData)?
                .Where(i => i != null)
                .ToArray());

            return jPartition?.ToString();
        }
    }
}
