using System.Diagnostics;
using System.Text;
using ExcelDataReader;
using Newtonsoft.Json.Linq;
using Songhay.Abstractions;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;

namespace Songhay.Social.Activities;

// ReSharper disable once InconsistentNaming
public class IExcelDataReaderActivity : IActivity
{
    static IExcelDataReaderActivity() => TraceSource = TraceSources
        .Instance
        .GetTraceSourceFromConfiguredName()
        .WithSourceLevels();

    static readonly TraceSource? TraceSource;

    public string DisplayHelp(ProgramArgs? args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if(args.HelpSet.Any()) return args.ToHelpDisplayText() ?? string.Empty;

        args.HelpSet.Add(ArgExcelFile, "The absolute path to Excel-file input.");
        args.HelpSet.Add(ArgPartitionRoot, "The absolute path to Excel-file partition output Directory.");
        args.HelpSet.Add(ArgPartitionSize, "The size of Excel-file output partitions.");

        return args.ToHelpDisplayText() ?? string.Empty;
    }

    public void Start(ProgramArgs? args)
    {
        TraceSource?.WriteLine($"{nameof(IExcelDataReaderActivity)} starting...");

        var (excelPath, partitionSize, partitionRoot) = ProcessArgs(args);

        PartitionRows(excelPath, partitionSize, partitionRoot);
    }

    internal void PartitionRows(string excelPath, int partitionSize, string partitionRoot)
    {
        TraceSource?.WriteLine($"{nameof(PartitionRows)}: opening `{excelPath}`...");

        if (partitionSize == 0) partitionSize = 10;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        do
        {
            var counter = 0;
            List<string> uriList = new();

            TraceSource?.TraceVerbose($"{nameof(PartitionRows)}: {nameof(reader.Name)}: {reader.Name}");

            while (reader.Read())
            {
                var uri = reader.GetString(0);
                TraceSource?.TraceVerbose($"{nameof(PartitionRows)}: {nameof(uri)}: {uri}");
                if (string.IsNullOrWhiteSpace(uri)) continue;

                uriList.Add(uri);
                ++counter;

                if (counter % partitionSize != 0) continue;

                TraceSource?.TraceVerbose($"{nameof(PartitionRows)}: partitioning [{nameof(partitionSize)}: {partitionSize}]...");

                SavePartition(uriList, excelPath, partitionRoot, reader.Name, counter);

                uriList.Clear();
            }

            TraceSource?.TraceVerbose($"{nameof(PartitionRows)}: partitioning [final partition count: {uriList.Count}]...");
            SavePartition(uriList, excelPath, partitionRoot, reader.Name, counter);

        } while (reader.NextResult());
    }

    internal static (string excelPath, int partitionSize, string partitionRoot) ProcessArgs(ProgramArgs? args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var excelPath = args.GetArgValue(ArgExcelFile);
        if (!File.Exists(excelPath))
        {
            throw new FileNotFoundException($"The expected Excel file, `{excelPath ?? "[null]"}`, is not here.");
        }

        string partitionRoot = args.GetArgValue(ArgPartitionRoot).ToReferenceTypeValueOrThrow();
        if (!Directory.Exists(partitionRoot))
        {
            throw new DirectoryNotFoundException($"The expected Excel-file partition root, `{partitionRoot}`, is not here.");
        }

        int partitionSize = Convert.ToInt32(args.GetArgValue(ArgPartitionSize));

        return (excelPath, partitionSize, partitionRoot);
    }

    internal static void SavePartition(IEnumerable<string> partition, string excelPath, string partitionRoot, string partitionDirectory, int partitionCount)
    {
        var jPartition = JArray.FromObject(partition);
        var excelFileName = Path.GetFileNameWithoutExtension(excelPath);
        var fileName = $"{excelFileName.ToLowerInvariant()}-{partitionDirectory.ToLowerInvariant()}-partition-{partitionCount:000}.json";

        partitionRoot = ProgramFileUtility.GetCombinedPath(partitionRoot, partitionDirectory);

        Directory.CreateDirectory(partitionRoot);

        var path = ProgramFileUtility.GetCombinedPath(partitionRoot, fileName);
        File.WriteAllText(path, jPartition.ToString());
    }

    internal const string ArgExcelFile = "--excel-file";
    internal const string ArgPartitionRoot = "--partition-root";
    internal const string ArgPartitionSize = "--partition-size";
}
