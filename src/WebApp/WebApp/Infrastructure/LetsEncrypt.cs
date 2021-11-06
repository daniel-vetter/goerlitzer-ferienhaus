using Certes;
using FluffySpoon.AspNet.EncryptWeMust;
using FluffySpoon.AspNet.EncryptWeMust.Certes;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WebApp.Infrastructure
{
    public static class LetsEncrypt
    {
        public static void AddLetsEncryptProvider(this IServiceCollection sc)
        {
            sc.AddFluffySpoonLetsEncrypt(new LetsEncryptOptions()
            {
                Email = "daniel.vetter86@gmail.com",
                UseStaging = false,
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

            var basePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "C:\\data\\LetsEncrypt"
                : "/data/LetsEncrypt";

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            sc.AddFluffySpoonLetsEncryptFileCertificatePersistence(Path.Combine(basePath, "certificate"));
            sc.AddFluffySpoonLetsEncryptFileChallengePersistence(Path.Combine(basePath, "challenge"));
        }

        public static void UseLetsEncryptProvider(this IApplicationBuilder app)
        {
            app.UseFluffySpoonLetsEncrypt();
        }
    }
}
