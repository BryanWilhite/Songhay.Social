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
        [InlineData(
            "What we know so far about South Africa&#x27;s largest ever data breach",
            "South Africa has suffered its largest data breach as millions of personal\nrecords of anyone, dead or alive, with a South African ID number (13 digit\nidentity number) have been leaked on the Internet. This was first revealed by\nsecurity consultant and researcher, Troy Hunt [https://twitter.com/troyhunt], on\n17 October 2017.\n\nHunt is also the founder of have i been pwned? [https://haveibeenpwned.com/], an\nonline service which allows you to check if you have an account, e-mail address\nor username, ",
            "What we know so far about South Africa&#x27;s largest ever data breach",
            "South Africa has suffered its largest data breach as millions of personal\nrecords of anyone, dead or alive, with a South African ID number (13 digit\nidentity number) have been leaked on the Internet. This was first revealed by\nsecurity consultant and researcher, Troy Hunt [https://twitter.com/troyhunt], on\n17 October 2017.\n\nHunt is also the founder of have i been pwned? [https://haveibeenpwned.com/], an\nonline service which allows you to check if you have an account, e-mail address\nor username, ",
            "What we know so far about South Africa&#x27;s largest ever data breach"
            )]
        [InlineData(
            "Scientists Shocked As Fisheries Collapse On West Coast: &#039;It&#039;s The Worst We&#039;ve Seen&quot;",
            "The Gulf of Alaska cod populations appears to have taken a nose-dive. Scientists are shocked at the collapse and starving fish, making this  the ",
            null,
            null,
            "Scientists Shocked As Fisheries Collapse On West Coast: &#039;It&#039;s The Worst We&#039;ve Seen&quot;"
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
                statusTitleAndDescription = string.Concat(statusTitleAndDescription.Substring(0, twitterCharacterLimit - 1), "…");
            }

            this._testOutputHelper.WriteLine($@"
after:

{statusTitleAndDescription}

{nameof(statusTitleAndDescription)} length: {statusTitleAndDescription.Length}
");
            Assert.True(statusTitleAndDescription.Length <= twitterCharacterLimit);
        }

        readonly ITestOutputHelper _testOutputHelper;
    }
}
