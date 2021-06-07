using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebApp.Infrastructure.SqliteRequestTracer
{
    public class TraceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestTraceWriter _backgroundWorker;

        public TraceMiddleware(RequestDelegate next, RequestTraceWriter backgroundWorker)
        {
            _next = next;
            _backgroundWorker = backgroundWorker;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();

            var request = new Request(
                Timestamp: DateTime.UtcNow,
                Method: context.Request.Method,
                Path: context.Request.Path,
                Host: context.Request.Host.ToString(),
                Protocol: context.Request.Protocol,
                QueryString: context.Request.QueryString.ToString(),
                StatusCode: context.Response.StatusCode,
                PerformanceBucket: GetPerformanceBucket(sw.Elapsed.TotalMilliseconds),
                Duration: sw.Elapsed.TotalMilliseconds
            );

            await _backgroundWorker.Write(request);
        }

        private string GetPerformanceBucket(double totalMilliseconds)
        {
            return totalMilliseconds switch
            {
                < 250 => "<250ms",
                < 500 => "250ms-500ms",
                < 1000 => "500ms-1sec",
                < 3000 => "1sec-3sec",
                < 7000 => "3sec-7sec",
                < 15000 => "7sec-15sec",
                < 30000 => "15sec-30sec",
                _ => ">30sec"
            };
        }
    }
}
