using ExcelDataReader;
using Songhay.Extensions;
using System.IO;
using System.Text;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests
{
    public class TweetBookContextTest
    {
        public TweetBookContextTest(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;
        }

        [Theory]
        [InlineData(@"./TweetBooks/TweetBook-{year}-{month}.xlsx")]
        public void ShouldReadTweetBook(string pathExpression)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var projectRoot = FrameworkAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var pathTemplate = new UriTemplate(pathExpression);

            var path = pathTemplate.BindByPosition(year, month)?.OriginalString;
            path = Path.Combine(projectInfo.FullName, path);
            path = Path.GetFullPath(path);
            Assert.True(File.Exists(path));

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            this._testOutputHelper.WriteLine(reader.GetString(1));
                        }
                    } while (reader.NextResult());
                }
            }
        }

        const string year = "2018";
        const string month = "05";

        readonly ITestOutputHelper _testOutputHelper;
    }
}
