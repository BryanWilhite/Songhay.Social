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
        [InlineData(@"https://codeopinionXXX.com/avoiding-nullreferenceexception/")]
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

        readonly ITestOutputHelper _testOutputHelper;
    }
}
