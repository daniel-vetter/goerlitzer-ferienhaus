using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebApp.Infrastructure.SqliteLogger
{
    [ProviderAlias("Sqlite")]
    public class LogProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();
        private readonly string _fileName;
        private readonly Task _processQueueTask;
        private readonly Channel<LogEntry> _channel;

        public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;

        public LogProvider(SqliteLoggerOptions options)
        {
            _fileName = options.FileName ?? throw new Exception("No logging file specified.");
            _channel = Channel.CreateUnbounded<LogEntry>();
            _processQueueTask = Task.Factory.StartNew(ProcessQueue, this, TaskCreationOptions.LongRunning);
        }

        private async Task ProcessQueue(object state)
        {
            await EnsureDbIsInitialized();
            while (await _channel.Reader.WaitToReadAsync())
            {
                using var db = await CreateConnection();
                using var t = await db.BeginTransactionAsync();
                while (_channel.Reader.TryRead(out var entry))
                {
                    using var cmd = db.CreateCommand();
                    cmd.CommandText = "INSERT INTO LogEntries (Timestamp, LogLevel, EventId, EventName, Message, Exception) VALUES (@Timestamp, @LogLevel, @EventId, @EventName, @Message, @Exception);";
                    cmd.Parameters.AddWithValue("@Timestamp", entry.Timestamp);
                    cmd.Parameters.AddWithValue("@LogLevel", entry.LogLevel);
                    cmd.Parameters.AddWithValue("@EventId", entry.EventId.Id == 0 ? DBNull.Value : entry.EventId.Id);
                    cmd.Parameters.AddWithValue("@EventName", (object)(entry.EventId.Name) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Message", entry.Message);
                    cmd.Parameters.AddWithValue("@Exception", (object)(entry.Exception) ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
                await t.CommitAsync();
            }
        }

        private async Task EnsureDbIsInitialized()
        {
            using var con = await CreateConnection();
            using var cmd = con.CreateCommand();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS LogEntries (
	                Id          INTEGER NOT NULL PRIMARY KEY,
                    Timestamp   TEXT NOT NULL,
                    LogLevel    INTEGER NOT NULL,
                    EventId     INTEGER NULL,
                    EventName   TEXT NULL,
   	                Message     TEXT NOT NULL,
                    Exception   TEXT NULL
                )
            ";

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<SqliteConnection> CreateConnection()
        {
            var con = new SqliteConnection($"Data Source={_fileName}");
            await con.OpenAsync();
            return con;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, new Logger(categoryName, this));
        }

        public void Dispose()
        {
            _channel.Writer.Complete();
            _processQueueTask.Wait(1500);
            _loggers.Clear();
        }

        internal void Write(LogEntry logEntry)
        {
            _channel.Writer.TryWrite(logEntry);
        }
    }

    public class SqliteLoggerOptions
    {
        public string FileName { get; set; }
        public LogLevel LogLevel { get; internal set; }
    }
}
