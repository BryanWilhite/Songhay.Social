using Microsoft.Extensions.Configuration;
using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using Songhay.Social.Models;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo(assemblyName: "Songhay.Social.Shell.Tests")]

namespace Songhay.Social.Shell
{
    class Program
    {
        internal static void DisplayCredits()
        {
            Console.Write(ProgramAssemblyUtility.GetAssemblyInfo(Assembly.GetExecutingAssembly(), true));
            Console.WriteLine(string.Empty);
            Console.WriteLine("Activities Assembly:");
            Console.Write(ProgramAssemblyUtility.GetAssemblyInfo(typeof(SocialActivitiesGetter).Assembly, true));
        }

        internal static async Task RunAsync(string[] args)
        {
            var configuration = ProgramUtility.LoadConfiguration(
                Directory.GetCurrentDirectory(),
                builderModifier => builderModifier.AddJsonFile($"./{AppScalars.ConventionalSettingsFile}")
            );

            TraceSources.ConfiguredTraceSourceName = configuration[DeploymentEnvironment.DefaultTraceSourceNameConfigurationKey];

            var traceSource = TraceSources
                .Instance
                .GetTraceSourceFromConfiguredName()
                .WithSourceLevels();

            var getter = new SocialActivitiesGetter(args);
            var activity = getter.GetActivity().WithConfiguration(configuration);

            if (getter.Args.IsHelpRequest())
            {
                Console.WriteLine(activity.DisplayHelp(getter.Args));
                return;
            }

            var json = await activity
                .StartConsoleActivityAsync<ProgramArgs, string>(getter.Args, traceSource)
                .ConfigureAwait(continueOnCapturedContext: false);

            if (string.IsNullOrWhiteSpace(json))
                throw new NullReferenceException(nameof(json));

            if (getter.Args.HasArg(argOutputFile, requiresValue: true))
            {
                var outputFile = getter.Args.GetArgValue(argOutputFile);

                File.WriteAllText(outputFile, json);
            }
        }

        internal const string argOutputFile = "--output-file";

        static async Task Main(string[] args)
        {
            DisplayCredits();

            await RunAsync(args);
        }
    }
}
