using Microsoft.Extensions.Logging;
using System;

namespace WebApp.Infrastructure.SqliteLogger
{
    public class Logger : ILogger
    {
        private readonly LogProvider _provider;
        private string _categoryName;

        public Logger(string categoryName, LogProvider provider)
        {
            _provider = provider;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _provider.MinLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            _provider.Write(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                CategoryName = _categoryName,
                LogLevel = logLevel,
                EventId = eventId,
                Message = message,
                Exception = exception?.ToString()
            });
        }
    }
}
