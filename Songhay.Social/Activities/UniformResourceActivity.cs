using HtmlAgilityPack;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Songhay.Social.Activities
{
    public class UniformResourceActivity : IActivity
    {
        static UniformResourceActivity() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();

        static TraceSource traceSource;

        public string DisplayHelp(ProgramArgs args)
        {
            throw new NotImplementedException();
        }

        public void Start(ProgramArgs args)
        {
            throw new NotImplementedException();
        }

        internal void GenerateTwitterPartitions(string [] uris)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var htmlWeb = new HtmlWeb().WithChromeishUserAgent();

            var jPartition = uris
                .Select(htmlWeb.ToSocialData)?
                .Where(i => i != null).ToArray();
        }
    }
}
