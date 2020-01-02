using HtmlAgilityPack;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests
{
    public class HtmlWebTests
    {
        public HtmlWebTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;
        }

        [Theory]
        [InlineData(@"https://codeopinion.com/avoiding-nullreferenceexception/")]
        public void ShouldLoad(string location)
        {
            //https://html-agility-pack.net/documentation
            //https://devhints.io/xpath

            var web = new HtmlWeb();

            var htmlDoc = web.Load(location);

            this._testOutputHelper.WriteLine($"{nameof(htmlDoc.DocumentNode)}: {htmlDoc.DocumentNode.InnerHtml}");

            var metaTwitterImage = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image:src']");
            this._testOutputHelper.WriteLine($"{nameof(metaTwitterImage)}: {metaTwitterImage?.OuterHtml ?? "[null]"}");

            var metaTwitterHandle = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:site']");
            this._testOutputHelper.WriteLine($"{nameof(metaTwitterHandle)}: {metaTwitterHandle?.OuterHtml ?? "[null]"}");

            var metaTwitterTitle = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']");
            this._testOutputHelper.WriteLine($"{nameof(metaTwitterTitle)}: {metaTwitterTitle?.OuterHtml ?? "[null]"}");

            var metaTwitterDescription = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']");
            this._testOutputHelper.WriteLine($"{nameof(metaTwitterDescription)}: {metaTwitterDescription?.OuterHtml ?? "[null]"}");

            var metaOgImage = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            this._testOutputHelper.WriteLine($"{nameof(metaOgImage)}: {metaOgImage?.OuterHtml ?? "[null]"}");

            var metaOgTitle = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            this._testOutputHelper.WriteLine($"{nameof(metaOgTitle)}: {metaOgTitle?.OuterHtml ?? "[null]"}");

            var metaOgDescription = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            this._testOutputHelper.WriteLine($"{nameof(metaOgDescription)}: {metaOgDescription?.OuterHtml ?? "[null]"}");

            var title = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            this._testOutputHelper.WriteLine($"{nameof(title)}: {title?.OuterHtml ?? "[null]"}");
        }

        [Theory]
        [InlineData(
            "Tesla has fired hundreds of employees after performance reviews &#8211; TechCrunch",
            "Tesla fired hundreds of employees following what the company said is an annual performance review process, the company told TechCrunch. The news was first reported by The Mercury News. “Like all companies, Tesla conducts an annual performance review during which a manager and employee discuss the results that were achieved, as well as how those results [&hellip;]",
            "Tesla has fired hundreds of employees after performance reviews &#8211; TechCrunch",
            "Tesla fired hundreds of employees following what the company said is an annual performance review process, the company told TechCrunch. The news was first reported by The Mercury News. “Like all companies, Tesla conducts an annual performance review during which a manager and employee discuss the r…",
            "Tesla has fired hundreds of employees after performance reviews &#8211; TechCrunch"
            )]
        public void ShouldTruncateStatus(string metaTwitterTitle,
            string metaTwitterDescription,
            string metaOgTitle,
            string metaOgDescription,
            string title)
        {
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

            var twitterCharacterLimit = 280 - 50; // 50 chars is the fudge factor (link shortener ~30 chars?; ~14 status chars)
            var length = statusTitleAndDescription.Length;

            this._testOutputHelper.WriteLine($@"
before:

{statusTitleAndDescription}

{nameof(statusTitleAndDescription)} length: {statusTitleAndDescription.Length}
");

            if (length > twitterCharacterLimit)
            {
                var delta = length - twitterCharacterLimit + 1;
                statusTitleAndDescription = string.Concat(statusTitleAndDescription.Substring(0, delta), "…");
            }

            this._testOutputHelper.WriteLine($@"
after:

{statusTitleAndDescription}

{nameof(statusTitleAndDescription)} length: {statusTitleAndDescription.Length}
");
        }

        readonly ITestOutputHelper _testOutputHelper;
    }
}
