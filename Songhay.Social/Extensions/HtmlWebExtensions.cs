using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Polly;
using Songhay.Diagnostics;
using Songhay.Extensions;
using System.Diagnostics;

namespace Songhay.Social.Extensions;

public static class HtmlWebExtensions
{
    static HtmlWebExtensions() => TraceSource = TraceSources
        .Instance
        .GetTraceSourceFromConfiguredName()
        .WithSourceLevels();

    static readonly TraceSource? TraceSource;

    public static async Task<JObject> ToSocialDataAsync(this HtmlWeb web, string location, AsyncPolicy retryPolicy)
    {
        ArgumentNullException.ThrowIfNull(web);
        if (string.IsNullOrWhiteSpace(location))
        {
            var anonCatch = new
            {
                errorType = nameof(NullReferenceException),
                errorMessage = $"The expected {nameof(location)} is not here.",
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

            return JObject.FromObject(anonCatch);
        }

        TraceSource?.WriteLine($"{nameof(ToSocialDataAsync)}: loading `{location}`...");

        HtmlDocument? htmlDoc = null;
        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                htmlDoc = await web.LoadFromWebAsync(location)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }).ConfigureAwait(continueOnCapturedContext: false);
        }
        catch (Exception ex)
        {
            var anonCatch = new
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

            return JObject.FromObject(anonCatch);
        }

        #region set anonymous properties:

        var documentNode = htmlDoc.ToReferenceTypeValueOrThrow().DocumentNode.ToReferenceTypeValueOrThrow();

        //FUNKYKB: tracing HTML can fill up text buffer fast when this member is called repeatedly:
        TraceSource?.TraceVerbose($"{nameof(HtmlDocument.DocumentNode)}: {(documentNode.HasChildNodes ? "[document has child nodes]" : "[no child notes]")}");

        var metaTwitterImageNode = documentNode.SelectSingleNode("//meta[@name='twitter:image:src']");
        TraceSource?.TraceVerbose($"{nameof(metaTwitterImageNode)}: {metaTwitterImageNode?.OuterHtml ?? "[null]"}");
        var metaTwitterImage = metaTwitterImageNode?.Attributes["content"]?.Value;

        var metaTwitterHandleNode = documentNode.SelectSingleNode("//meta[@name='twitter:site']");
        TraceSource?.TraceVerbose($"{nameof(metaTwitterHandleNode)}: {metaTwitterHandleNode?.OuterHtml ?? "[null]"}");
        var metaTwitterHandle = metaTwitterHandleNode?.Attributes["content"]?.Value;

        var metaTwitterTitleNode = documentNode.SelectSingleNode("//meta[@name='twitter:title']");
        TraceSource?.TraceVerbose($"{nameof(metaTwitterTitleNode)}: {metaTwitterTitleNode?.OuterHtml ?? "[null]"}");
        var metaTwitterTitle = metaTwitterTitleNode?.Attributes["content"]?.Value;

        var metaTwitterDescriptionNode = documentNode.SelectSingleNode("//meta[@name='twitter:description']");
        TraceSource?.TraceVerbose($"{nameof(metaTwitterDescriptionNode)}: {metaTwitterDescriptionNode?.OuterHtml ?? "[null]"}");
        var metaTwitterDescription = metaTwitterDescriptionNode?.Attributes["content"]?.Value;

        var metaOgImageNode = documentNode.SelectSingleNode("//meta[@property='og:image']");
        TraceSource?.TraceVerbose($"{nameof(metaOgImageNode)}: {metaOgImageNode?.OuterHtml ?? "[null]"}");
        var metaOgImage = metaOgImageNode?.Attributes["content"]?.Value;

        var metaOgTitleNode = documentNode.SelectSingleNode("//meta[@property='og:title']");
        TraceSource?.TraceVerbose($"{nameof(metaOgTitleNode)}: {metaOgTitleNode?.OuterHtml ?? "[null]"}");
        var metaOgTitle = metaOgTitleNode?.Attributes["content"]?.Value;

        var metaOgDescriptionNode = documentNode.SelectSingleNode("//meta[@property='og:description']");
        TraceSource?.TraceVerbose($"{nameof(metaOgDescriptionNode)}: {metaOgDescriptionNode?.OuterHtml ?? "[null]"}");
        var metaOgDescription = metaOgDescriptionNode?.Attributes["content"]?.Value;

        var titleNode = documentNode.SelectSingleNode("//head/title");
        TraceSource?.TraceVerbose($"{nameof(titleNode)}: {titleNode?.OuterHtml ?? "[null]"}");
        var title = titleNode?.InnerText;

        var statusTitle = ((!string.IsNullOrWhiteSpace(metaTwitterTitle)) ? metaTwitterTitle
            :
            (!string.IsNullOrWhiteSpace(metaOgTitle)) ? metaOgTitle : title)?.Trim();

        var statusDescription = ((!string.IsNullOrWhiteSpace(metaTwitterDescription)) ? metaTwitterDescription
            :
            metaOgDescription)?.Trim();

        var statusTitleAndDescription = HtmlEntity.DeEntitize($@"
{statusTitle ?? string.Empty}

{statusDescription ?? string.Empty}
".Trim());

        var tag = "#rxa 👁⚙🌩";
        var twitterCharacterLimit = 280 - 50; // 50 chars is the fudge factor (link shortener ~30 chars?; ~14 status chars)
        var length = statusTitleAndDescription.Length;

        if (length > twitterCharacterLimit)
        {
            statusTitleAndDescription = string.Concat(statusTitleAndDescription.Substring(0, twitterCharacterLimit - 1), "…");
        }

        var status = $@"
{statusTitleAndDescription}

{location}

{tag}
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

    public static HtmlWeb WithChromeishUserAgent(this HtmlWeb web)
    {
        if (web == null) throw new NullReferenceException($"The expected {nameof(HtmlWeb)} is not here.");

        // https://www.whatismybrowser.com/detect/what-http-headers-is-my-browser-sending
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";
        return web;
    }
}