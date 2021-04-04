using ExcelDataReader;
using Songhay.Extensions;
using System.IO;
using System.Text;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Abstractions;

namespace Songhay.Social.Shell.Tests
{
    public class TweetBookContextTests
    {
        public TweetBookContextTests(ITestOutputHelper helper)
        {
            this._testOutputHelper = helper;
        }

        [Theory]
        [InlineData(@"./TweetBooks/TweetBook-{year}-{month}.xlsx")]
        public void ShouldReadTweetBook(string pathExpression)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var projectRoot = ProgramAssemblyUtility.GetPathFromAssembly(this.GetType().Assembly, "../../../");
            var projectInfo = new DirectoryInfo(projectRoot);
            Assert.True(projectInfo.Exists);

            var pathTemplate = new UriTemplate(pathExpression);

            var path = pathTemplate.BindByPosition(year, month)?.OriginalString;
            path = projectInfo.ToCombinedPath(path);
            Assert.True(File.Exists(path));

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        this._testOutputHelper.WriteLine($"{nameof(reader.Name)}: {reader.Name}");

                        while (reader.Read())
                        {
                            this._testOutputHelper.WriteLine(reader.GetString(0));
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
