using DocAggregator.API.Core;
using DocAggregator.API.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocAggregator.API
{
    public class Startup
    {
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var editorService = new Infrastructure.OpenXMLProcessing.EditorService(_loggerFactory.CreateLogger<IEditorService>().Adapt())
            {
                TemplatesDirectory = Configuration["Editor:TemplatesDir"],
                TemporaryOutputDirectory = Configuration["Editor:OutputDir"],
                LibreOfficeFolder = Configuration["Editor:LibreOffice"],
                Scripts = Configuration["Editor:Scripts"],
            };
            editorService.Initialize();
            services.AddSingleton<IEditorService>(editorService);
            services.AddSingleton<IClaimRepository>(new Infrastructure.OracleManaged.ClaimRepository());
            var fieldRepository = new Infrastructure.OracleManaged.MixedFieldRepository(_loggerFactory.CreateLogger<IMixedFieldRepository>().Adapt())
            {
                QueriesSource = Configuration["DB:QueriesFile"],
                Server = Configuration["DB:DataSource"],
                Username = Configuration["DB:UserID"],
                Password = Configuration["DB:Password"],
            };
            services.AddSingleton<IMixedFieldRepository>(fieldRepository);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts();
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
