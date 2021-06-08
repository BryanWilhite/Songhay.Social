using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using Newtonsoft.Json.Linq;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;

namespace Songhay.Social.Activities
{
    public class IExcelDataReaderActivity : IActivity
    {
        static IExcelDataReaderActivity() => traceSource = TraceSources
            .Instance
            .GetTraceSourceFromConfiguredName()
            .WithSourceLevels();

        static TraceSource traceSource;

        public string DisplayHelp(ProgramArgs args)
        {
            if(args.HelpSet.Any()) return args.ToHelpDisplayText();

            args.HelpSet.Add(argExcelFile, "The absolute path to Excel-file input.");
            args.HelpSet.Add(argPartitionRoot, "The absolute path to Excel-file partition output Directory.");
            args.HelpSet.Add(argPartitionSize, "The size of Excel-file output partitions.");

            return args.ToHelpDisplayText();
        }

        public void Start(ProgramArgs args)
        {
            traceSource?.WriteLine($"{nameof(IExcelDataReaderActivity)} starting...");

            if (
                    args.HasArg(argExcelFile, requiresValue: true) &&
                    args.HasArg(argPartitionRoot, requiresValue: true) &&
                    args.HasArg(argPartitionSize, requiresValue: true)
                )
            {
                var excelPath = args.GetArgValue(argExcelFile);
                if (!File.Exists(excelPath))
                {
                    throw new FileNotFoundException($"The expected Excel file, `{excelPath ?? "[null]"}`, is not here.");
                }

                string partitionRoot = args.GetArgValue(argPartitionRoot);
                if (!Directory.Exists(partitionRoot))
                {
                    throw new DirectoryNotFoundException($"The expected Excel-file partition root, `{partitionRoot ?? "[null]"}`, is not here.");
                }

                int partitionSize = Convert.ToInt32(args.GetArgValue(argPartitionSize));

                this.PartitionRows(excelPath, partitionSize, partitionRoot);
            }
        }

        internal void PartitionRows(string excelPath, int partitionSize, string partitionRoot)
        {
            traceSource?.WriteLine($"{nameof(PartitionRows)}: opening `{excelPath}`...");

            if (partitionSize == 0) partitionSize = 10;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            do
            {
                var counter = 0;
                var uriList = new List<string>();

                traceSource?.TraceVerbose($"{nameof(PartitionRows)}: {nameof(reader.Name)}: {reader.Name}");

                while (reader.Read())
                {
                    var uri = reader.GetString(0);
                    traceSource?.TraceVerbose($"{nameof(PartitionRows)}: {nameof(uri)}: {uri ?? "[null]"}");
                    if (string.IsNullOrWhiteSpace(uri)) continue;

                    uriList.Add(uri);
                    ++counter;

                    if (counter % partitionSize == 0)
                    {
                        traceSource?.TraceVerbose($"{nameof(PartitionRows)}: partitioning [{nameof(partitionSize)}: {partitionSize}]...");

                        this.SavePartition(uriList, partitionRoot, reader.Name, counter);

                        uriList.Clear();
                    }
                }

                traceSource?.TraceVerbose($"{nameof(PartitionRows)}: partitioning [final partition count: {uriList?.Count ?? 0}]...");
                this.SavePartition(uriList, partitionRoot, reader.Name, counter);

            } while (reader.NextResult());
        }

        internal void SavePartition(IEnumerable<string> partition, string partitionRoot, string partitionDirectory, int partitionCount)
        {
            var jPartition = JArray.FromObject(partition);
            var fileName = $"excel-row-partition-{partitionCount:00}.json";

            partitionRoot = ProgramFileUtility.GetCombinedPath(partitionRoot, partitionDirectory);

            Directory.CreateDirectory(partitionRoot);

            var path = ProgramFileUtility.GetCombinedPath(partitionRoot, fileName);
            File.WriteAllText(path, jPartition.ToString());
        }

        internal const string argExcelFile = "--excel-file";
        internal const string argPartitionRoot = "--partition-root";
        internal const string argPartitionSize = "--partition-size";
    }
}