using Microsoft.AspNetCore.DataProtection;
using System.Runtime.InteropServices;
using WebApp;
using WebApp.Infrastructure;
using WebApp.Infrastructure.SqliteLogger;
using WebApp.Infrastructure.SqliteRequestTracer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages().AddRazorPagesOptions(x =>
{
    x.Conventions.Add(new KebaberizeUrls());
    x.Conventions.AddPageRoute("/NotFound", "{*url}");
});

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("/data/DataProtectionKeys"));

builder.Services.AddWebOptimizer(x =>
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

builder.Services.AddLogging(b =>
{
    b.AddSqlite(builder.Configuration.GetSection("Logging"));
});

builder.Services.AddSingleton<IMailSender, MailSender>();
builder.Services.AddHealthChecks().AddCheck<MailSender>("MailSender");
builder.Services.Configure<AppOptions>(builder.Configuration);
builder.Services.AddRequestTrace(builder.Configuration.GetSection("RequestTrace"));

var app = builder.Build();

app.UseRequestTrace();
if (app.Services.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
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
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapHealthChecks("/status");

app.Run();