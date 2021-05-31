using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApp.Infrastructure;

namespace WebApp
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_webHostEnvironment.IsProduction())
                services.AddLetsEncryptProvider();

            services.AddRazorPages().AddRazorPagesOptions(x => 
            {
                x.Conventions.Add(new KebaberizeUrls());
                x.Conventions.AddPageRoute("/NotFound", "{*url}");
            });

            services.AddWebOptimizer(x =>
            {
                x.MinifyJsFiles("scripts/**/*.js");
                x.MinifyCssFiles("css/**/*.css");
                x.AddCssBundle("/vendor/bundle.css",
                    "vendor/bootstrap/bootstrap.min.css",
                    "vendor/cookieconsent/cookieconsent.min.css");
                x.AddJavaScriptBundle("/vendor/bundle.js",
                    "vendor/jquery/jquery.min.js",
                    "vendor/popper/popper.min.js",
                    "vendor/bootstrap/bootstrap.min.js",
                    "vendor/cookieconsent/cookieconsent.min.js");
            });

            services.AddSingleton<IMailSender, MailSender>();
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            if (env.IsProduction())
                app.UseLetsEncryptProvider();

            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/status");
            });
        }
    }
}
