using ExcelDataReader;
using HtmlAgilityPack;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Social.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Songhay.Social.Extensions
{
    public static class FileInfoExtensions
    {
        static FileInfoExtensions() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();

        static TraceSource traceSource;

        public static void PartitionSocialData(this FileInfo fileInfo, int partitionSize, Action<SocialPartition> partitionAction)
        {
            if (fileInfo == null) throw new NullReferenceException($"The expected {nameof(FileInfo)} is not here.");
            if (partitionSize == 0) partitionSize = 10;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var htmlWeb = new HtmlWeb().WithChromeishUserAgent();

            traceSource?.WriteLine($"{nameof(PartitionSocialData)}: opening `{fileInfo.FullName}`...");

            using (var stream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        var counter = 0;
                        var uriList = new List<string>();

                        traceSource?.TraceVerbose($"{nameof(PartitionSocialData)}: {nameof(reader.Name)}: {reader.Name}");

                        while (reader.Read())
                        {
                            var uri = reader.GetString(0);
                            traceSource?.TraceVerbose($"{nameof(PartitionSocialData)}: {nameof(uri)}: {uri ?? "[null]"}");
                            if (string.IsNullOrWhiteSpace(uri)) continue;

                            uriList.Add(uri);
                            ++counter;

                            if (counter % partitionSize == 0)
                            {
                                traceSource?.TraceVerbose($"{nameof(PartitionSocialData)}: partitioning [{nameof(partitionSize)}: {partitionSize}]...");
                                var jPartition = uriList.Select(htmlWeb.ToSocialData)?.Where(i => i != null).ToArray();
                                partitionAction?.Invoke(new SocialPartition { Data = jPartition, GroupName = reader.Name, PartitionOrdinal = counter });
                                uriList.Clear();
                            }
                        }

                        var jPartitionFinal = uriList.Select(htmlWeb.ToSocialData)?.Where(i => i != null).ToArray();
                        traceSource?.TraceVerbose($"{nameof(PartitionSocialData)}: partitioning [final count: {jPartitionFinal?.Count() ?? 0}]...");
                        partitionAction?.Invoke(new SocialPartition { Data = jPartitionFinal, GroupName = reader.Name, PartitionOrdinal = counter });

                    } while (reader.NextResult());
                }
            }
        }
    }
}
