using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Infrastructure.SqliteRequestTracer
{
    public static class TraceExtension
    {
        public static IServiceCollection AddRequestTrace(this IServiceCollection sc, IConfiguration configuration)
        {
            sc.AddSingleton<RequestTraceWriter>();
            sc.AddHostedService<RequestTraceWriterBackgroundServer>();
            sc.Configure<RequestTraceOptions>(configuration);
            return sc;
        }

        public static IApplicationBuilder UseRequestTrace(this IApplicationBuilder b)
        {
            b.UseMiddleware<TraceMiddleware>();   
            return b;
        }
    }

    public class RequestTraceOptions
    {
        public string FileName { get; set; }
    }
}
