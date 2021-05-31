using Certes;
using FluffySpoon.AspNet.LetsEncrypt;
using FluffySpoon.AspNet.LetsEncrypt.Certes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WebApp.Infrastructure;

namespace WebApp
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
            services.AddFluffySpoonLetsEncrypt(new LetsEncryptOptions()
            {
                Email = "daniel.vetter86@gmail.com",
                UseStaging = true,
                Domains = new[] { "goerlitzer-ferienhaus.de" },
                TimeUntilExpiryBeforeRenewal = TimeSpan.FromDays(30),
                CertificateSigningRequest = new CsrInfo()
                {
                    CountryName = "Germany",
                    Locality = "Sachsen",
                    Organization = "privat",
                    OrganizationUnit = "",
                    State = "Sachsen"
                }
            });

            services.AddFluffySpoonLetsEncryptFileCertificatePersistence();
            services.AddFluffySpoonLetsEncryptFileChallengePersistence();

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

            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseFluffySpoonLetsEncrypt();
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
