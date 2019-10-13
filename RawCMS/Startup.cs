﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Min�</author>
// <autogenerated>true</autogenerated>
//******************************************************************************

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using RawCMS.Library.Core;
using RawCMS.Library.Core.Helpers;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RawCMS
{
    public class Startup
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private AppEngine appEngine;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            this.loggerFactory = loggerFactory;
            this.logger = logger;

            var path = ApplicationLogger.GetConfigPath(env.EnvironmentName);
            loggerFactory.AddDebug();
            loggerFactory.AddNLog();
            logger.LogInformation($"Starting RawCMS, environment={env.EnvironmentName}");
            env.ConfigureNLog(path);

            ApplicationLogger.SetLogFactory(loggerFactory);

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            appEngine.InvokeConfigure(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseMvc();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{collection?}/{id?}");
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            appEngine.RegisterPluginsMiddleweares(app);

            app.UseStaticFiles();

            app.UseWelcomePage();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var ass = new List<Assembly>();
            var builder = services.AddMvc();
            var pluginPath = Configuration.GetValue<string>("PluginPath");
            List<Assembly> allAssembly = AssemblyHelper.GetAllAssembly();

            ReflectionManager rm = new ReflectionManager(allAssembly);

            appEngine = AppEngine.Create(
                pluginPath,
                loggerFactory.CreateLogger<AppEngine>(),
                rm, services, Configuration);

            appEngine.InvokeConfigureServices(ass, builder, services, Configuration);

            foreach (var a in ass.Distinct())
            {
                builder.AddApplicationPart(a).AddControllersAsServices();
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Web API", Version = "v1" });
                //x.IncludeXmlComments(AppContext.BaseDirectory + "YourProject.Api.xml");
                c.IgnoreObsoleteProperties();
                c.IgnoreObsoleteActions();
                c.DescribeAllEnumsAsStrings();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

            //Invoke appEngine after service configuration

            appEngine.InvokePostConfigureServices(services);
        }
    }
}