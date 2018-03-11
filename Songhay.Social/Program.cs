using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(assemblyName: "Songhay.GenericWeb.Tests")]

namespace Songhay.GenericWeb
{
    /// <summary>
    /// Defines the conventional ASP.NET Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The conventional main method.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            GetWebHostBuilder(args, builderAction: null).Build().Run();
        }

        internal static IWebHostBuilder GetWebHostBuilder() =>
            GetWebHostBuilder(args: null, builderAction: null)
            ;

        internal static IWebHostBuilder GetWebHostBuilder(string[] args, Action<WebHostBuilderContext, IConfigurationBuilder> builderAction)
            => WebHost
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    builderAction?.Invoke(builderContext, configBuilder);
                    configBuilder.AddJsonFile("app-settings.songhay-system.json", optional: false);
                })
                .UseStartup<Startup>()
                ;
    }
}
