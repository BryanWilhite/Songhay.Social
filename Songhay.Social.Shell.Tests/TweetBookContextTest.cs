using ExcelDataReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Songhay.Extensions;
using System.IO;
using System.Text;
using Tavis.UriTemplates;

namespace Songhay.Social.Shell.Tests
{
    [TestClass]
    public class TweetBookContextTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestProperty("pathTemplate", @"./TweetBooks/TweetBook-{year}-{month}.xlsx")]
        public void ShouldReadTweetBook()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var projectDirectoryInfo = this.TestContext.ShouldGetProjectDirectoryInfo(this.GetType());

            #region test properties:

            var pathTemplate = new UriTemplate(this.TestContext.Properties["pathTemplate"].ToString());

            #endregion

            var path = pathTemplate.BindByPosition(year, month)?.OriginalString;
            path = Path.Combine(projectDirectoryInfo.FullName, path);
            path = Path.GetFullPath(path);
            this.TestContext.ShouldFindFile(path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            this.TestContext.WriteLine(reader.GetString(1));
                        }
                    } while (reader.NextResult());
                }
            }
        }

        const string year = "2018";
        const string month = "05";
    }
}
