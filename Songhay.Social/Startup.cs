﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Songhay.Models;

namespace Songhay.Social
{
    /// <summary>
    /// Conventional ASP.NET Core Startup definition
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var meta = new ProgramMetadata();
            this.Configuration.Bind(nameof(ProgramMetadata), meta);

            services
                .AddCors(options =>
                {
                    options.AddPolicy(defaultCorsPolicyName,
                        builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
                })
                .AddSingleton(typeof(ProgramMetadata), meta)
                .AddMvc()
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseCors(defaultCorsPolicyName)
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseMvc()
                ;
        }

        const string defaultCorsPolicyName = "CorsPolicy";

        readonly IHostingEnvironment hostingEnvironment;
    }
}
