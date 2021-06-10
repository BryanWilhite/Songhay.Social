using Songhay.Diagnostics;
using Songhay.Extensions;
using Songhay.Models;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        internal static void Run(string[] args)
        {
            var configuration = ProgramUtility.LoadConfiguration(
                Directory.GetCurrentDirectory()
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

            activity.StartConsoleActivity(getter.Args, traceSource);
        }

        static void Main(string[] args)
        {
            DisplayCredits();

            Run(args);
        }
    }
}
