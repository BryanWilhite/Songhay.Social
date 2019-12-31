using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Songhay.Diagnostics;
using Songhay.Extensions;
using System;
using System.Diagnostics;

namespace Songhay.Social.Extensions
{
    public static class HtmlWebExtensions
    {
        static HtmlWebExtensions() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();

        static TraceSource traceSource;

        public static JObject ToSocialData(this HtmlWeb web, string location)
        {
            if (web == null) throw new NullReferenceException($"The expected {nameof(HtmlWeb)} is not here.");
            if (string.IsNullOrWhiteSpace(location)) throw new NullReferenceException($"The expected {nameof(location)} is not here.");

            traceSource?.WriteLine($"{nameof(ToSocialData)}: loading `{location}`...");
            var htmlDoc = web.Load(location);

            traceSource?.TraceVerbose($"{nameof(HtmlDocument.DocumentNode)}: {htmlDoc?.DocumentNode.InnerHtml ?? "[null]"}");

            var metaTwitterImage = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image:src']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterImage)}: {metaTwitterImage?.OuterHtml ?? "[null]"}");

            var metaTwitterHandle = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:site']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterHandle)}: {metaTwitterHandle?.OuterHtml ?? "[null]"}");

            var metaTwitterTitle = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterTitle)}: {metaTwitterTitle?.OuterHtml ?? "[null]"}");

            var metaTwitterDescription = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterDescription)}: {metaTwitterDescription?.OuterHtml ?? "[null]"}");

            var metaOgImage = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            traceSource?.TraceVerbose($"{nameof(metaOgImage)}: {metaOgImage?.OuterHtml ?? "[null]"}");

            var metaOgTitle = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            traceSource?.TraceVerbose($"{nameof(metaOgTitle)}: {metaOgTitle?.OuterHtml ?? "[null]"}");

            var metaOgDescription = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            traceSource?.TraceVerbose($"{nameof(metaOgDescription)}: {metaOgDescription?.OuterHtml ?? "[null]"}");

            var title = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            traceSource?.TraceVerbose($"{nameof(title)}: {title?.OuterHtml ?? "[null]"}");

            var anon = new
            {
                isPublished = false,
                location,
                metaTwitterImage,
                metaTwitterHandle,
                metaTwitterTitle,
                metaTwitterDescription,
                metaOgImage,
                metaOgTitle,
                metaOgDescription,
                title
            };

            return JObject.FromObject(anon);
        }
    }
}
