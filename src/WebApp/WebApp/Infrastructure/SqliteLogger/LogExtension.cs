using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace WebApp.Infrastructure.SqliteLogger
{
    public static class LogExtension
    {
        public static ILoggingBuilder AddSqlite(this ILoggingBuilder builder, string fileName, LogLevel logLevel = LogLevel.Information)
        {
            builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, LogProvider>(_ =>
            {
                return new LogProvider(new SqliteLoggerOptions
                {
                    FileName = fileName,
                    LogLevel = logLevel
                });
            }));
            return builder;
        }

        public static ILoggingBuilder AddSqlite(this ILoggingBuilder builder, IConfiguration configuration)
        {
            var config = configuration.GetSection("Sqlite");
            var fileName = config.GetSection("FileName").Value;
            if (fileName == null)
                return null;


            var minLevellStr = config.GetSection("LogLevel").Value;
            var minLevel = LogLevel.Information;
            if (minLevellStr != null)
                minLevel = Enum.Parse<LogLevel>(minLevellStr);

            builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, LogProvider>(_ =>
            {
                return new LogProvider(new SqliteLoggerOptions
                {
                    FileName = fileName,
                    LogLevel = minLevel
                });
            }));
            return builder;
        }


    }
}
