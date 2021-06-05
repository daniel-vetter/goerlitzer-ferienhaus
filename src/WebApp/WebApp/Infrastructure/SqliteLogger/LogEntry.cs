using Microsoft.Extensions.Logging;
using System;

namespace WebApp.Infrastructure.SqliteLogger
{
    internal class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }
        public string EventName { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public DateTime Timestamp { get; set; }
        public string CategoryName { get; set; }
        public string State { get; internal set; }
    }
}
