using DocAggregator.API.Core;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var editorService = new Infrastructure.OfficeInterop.WordService()
            {
                TemplatesDirectory = Configuration["Editor:TemplatesDir"],
                TemporaryOutputDirectory = Configuration["Editor:OutputDir"],
            } as IEditorService<IDocument>;
            services.AddSingleton<IEditorService<IDocument>>(editorService);
            services.AddSingleton<IClaimRepository>(new Infrastructure.OracleManaged.ClaimRepository());
            var fieldRepository = new Infrastructure.OracleManaged.MixedFieldRepository()
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
