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

            HtmlDocument htmlDoc = null;
            try
            {
                htmlDoc = web.Load(location);
            }
            catch (Exception ex)
            {
                traceSource?.TraceError(ex);
                var anonError = new
                {
                    errorType = ex.GetType().Name,
                    errorMessage = ex.Message,
                    hasLoadError = true,
                    isPublished = false,
                    location,
                    metaTwitterImage = string.Empty,
                    metaTwitterHandle = string.Empty,
                    metaTwitterTitle = string.Empty,
                    metaTwitterDescription = string.Empty,
                    metaOgImage = string.Empty,
                    metaOgTitle = string.Empty,
                    metaOgDescription = string.Empty,
                    status = string.Empty,
                    title = string.Empty
                };

                return JObject.FromObject(anonError);
            }

            #region set anonymous properties:

            //FUNKYKB: tracing HTML can fill up text buffer fast when this member is called repeatedly:
            traceSource?.TraceVerbose($"{nameof(HtmlDocument.DocumentNode)}: {((bool)htmlDoc?.DocumentNode?.HasChildNodes ? "[document has child nodes]" : "[no child notes]")}");

            var metaTwitterImageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image:src']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterImageNode)}: {metaTwitterImageNode?.OuterHtml ?? "[null]"}");
            var metaTwitterImage = metaTwitterImageNode?.Attributes["content"]?.Value;

            var metaTwitterHandleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:site']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterHandleNode)}: {metaTwitterHandleNode?.OuterHtml ?? "[null]"}");
            var metaTwitterHandle = metaTwitterHandleNode?.Attributes["content"]?.Value;

            var metaTwitterTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterTitleNode)}: {metaTwitterTitleNode?.OuterHtml ?? "[null]"}");
            var metaTwitterTitle = metaTwitterTitleNode?.Attributes["content"]?.Value;

            var metaTwitterDescriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']");
            traceSource?.TraceVerbose($"{nameof(metaTwitterDescriptionNode)}: {metaTwitterDescriptionNode?.OuterHtml ?? "[null]"}");
            var metaTwitterDescription = metaTwitterDescriptionNode?.Attributes["content"]?.Value;

            var metaOgImageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            traceSource?.TraceVerbose($"{nameof(metaOgImageNode)}: {metaOgImageNode?.OuterHtml ?? "[null]"}");
            var metaOgImage = metaOgImageNode?.Attributes["content"]?.Value;

            var metaOgTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            traceSource?.TraceVerbose($"{nameof(metaOgTitleNode)}: {metaOgTitleNode?.OuterHtml ?? "[null]"}");
            var metaOgTitle = metaOgTitleNode?.Attributes["content"]?.Value;

            var metaOgDescriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            traceSource?.TraceVerbose($"{nameof(metaOgDescriptionNode)}: {metaOgDescriptionNode?.OuterHtml ?? "[null]"}");
            var metaOgDescription = metaOgDescriptionNode?.Attributes["content"]?.Value;

            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            traceSource?.TraceVerbose($"{nameof(titleNode)}: {titleNode?.OuterHtml ?? "[null]"}");
            var title = titleNode?.InnerText;

            var statusTitle = (!string.IsNullOrWhiteSpace(metaTwitterTitle)) ? metaTwitterTitle
                :
                (!string.IsNullOrWhiteSpace(metaOgTitle)) ? metaOgTitle : title;

            var statusDescription = (!string.IsNullOrWhiteSpace(metaTwitterDescription)) ? metaTwitterDescription
                :
                metaOgDescription;

            var status = $@"
{HtmlEntity.DeEntitize(statusTitle)}

{HtmlEntity.DeEntitize(statusDescription ?? string.Empty)}

{location}

#rxa 👁⚙🌩
".Trim();

            #endregion

            var anon = new
            {
                errorType = string.Empty,
                errorMessage = string.Empty,
                hasLoadError = false,
                isPublished = false,
                location,
                metaTwitterImage,
                metaTwitterHandle,
                metaTwitterTitle,
                metaTwitterDescription,
                metaOgImage,
                metaOgTitle,
                metaOgDescription,
                status,
                title
            };

            return JObject.FromObject(anon);
        }
    }
}
